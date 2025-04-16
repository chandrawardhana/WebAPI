using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Organizations;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.ViewModels.Organizations;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;


namespace Saga.Mediator.Organizations.TitleMediator;

#region "Get List Title"
#region "Query"
    public sealed record GetTitlesQuery(Expression<Func<Title, bool>>[] wheres) : IRequest<TitleList>;
#endregion
#region "Handler"
    public sealed class GetTitlesQueryHandler : IRequestHandler<GetTitlesQuery, TitleList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetTitlesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TitleList> Handle(GetTitlesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Titles.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                              .ForEach(x =>
                              {
                                  queries = queries.Where(x);
                              });
            var titles = await queries.ToListAsync();
            var viewModel = new TitleList
            {
                Titles = _mapper.Map<IEnumerable<Title>>(titles)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetTitlesMapper : Profile
    {
        public GetTitlesMapper()
        {
            CreateMap<Title, TitleList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Title With Pagination"
#region "Query"
    public sealed record GetTitlesPaginationQuery(Expression<Func<Title, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Title>>;
#endregion
#region "Handler"
    public sealed class GetTitlesPaginationQueryHandler : IRequestHandler<GetTitlesPaginationQuery, PaginatedList<Title>>
    {
        private readonly IDataContext _context;

        public GetTitlesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Title>> Handle(GetTitlesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Titles.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(p => EF.Functions.ILike(p.Code, $"%{search}%") || EF.Functions.ILike(p.Name, $"%{search}%") || EF.Functions.ILike(p.Description, $"%{search}%"));
            }

            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });
            var titles = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
            titles.Items.ForEach(x =>
            {
                x.Company = _context.Companies.FirstOrDefault(f => f.Key == x.CompanyKey);
            });
            return await Task.FromResult(titles);
        }
    }
#endregion
#endregion

#region "Get By Id Title"
#region "Query"
    public sealed record GetTitleQuery(Guid Key) : IRequest<TitleForm>;
#endregion
#region "Handler"
    public sealed class GetTitleQueryHandler : IRequestHandler<GetTitleQuery, TitleForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetTitleQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TitleForm> Handle(GetTitleQuery request, CancellationToken cancellationToken)
        {
            var title = await _context.Titles.FirstOrDefaultAsync(t => t.Key == request.Key);
            if (title == null)
            {
                throw new InvalidOperationException("Title not found or has been deleted.");
            }

            var company = (title.CompanyKey != Guid.Empty)
                                ? await _context.Companies.FirstOrDefaultAsync(c => c.Key == title.CompanyKey) : null;
            title.Company = company;
            return _mapper.Map<TitleForm>(title);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetTitleMapper : Profile
    {
        public GetTitleMapper()
        {
            CreateMap<Title, TitleForm>().ReverseMap()
                    .ForMember(dest => dest.CompanyKey, opt => opt.MapFrom(src => src.CompanyKey))
                    .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company));
        }
    }
#endregion
#endregion

#region "Save Title"
#region "Command"
    public sealed record SaveTitleCommand(TitleDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveTitleCommandHandler : IRequestHandler<SaveTitleCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<TitleDto> _validator;

        public SaveTitleCommandHandler(IDataContext context, IMapper mapper, IValidator<TitleDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }
        public async Task<Result> Handle(SaveTitleCommand command, CancellationToken cancellationToken)
        {
            try
            {
                ValidationResult validator = await _validator.ValidateAsync(command.Form);
                if (!validator.IsValid)
                {
                    var failures = validator.Errors
                                                .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                                .ToList();
                    return Result.Failure(failures);
                }

                if (command.Form.Key == Guid.Empty || command.Form.Key == null)
                {
                    //Create Title
                    var title = _mapper.Map<Title>(command.Form);
                    title.Key = Guid.NewGuid();

                    _context.Titles.Add(title);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                } 
                else
                {
                    var title = _mapper.Map<Title>(command.Form);
                    if (title == null)
                    {
                        throw new InvalidOperationException("Title not found.");
                    }
                    _context.Titles.Update(title);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
            return Result.Success();
        }
    }
#endregion
#region "Mapper"
    public sealed class SaveTitleMapper : Profile
    {
        public SaveTitleMapper()
        {
            CreateMap<Title, TitleDto>().ReverseMap();
            CreateMap<Title, SaveTitleCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Title"
#region "Command"
    public sealed record DeleteTitleCommand(Guid Key) : IRequest<Result<Title>>;
#endregion
#region "Handler"
    public sealed class DeleteTitleCommandHandler : IRequestHandler<DeleteTitleCommand, Result<Title>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteTitleCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Title>> Handle(DeleteTitleCommand command, CancellationToken cancellationToken)
        {
            var title = await _context.Titles.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (title == null)
                {
                    throw new Exception("Title not found");
                }
                _mapper.Map<Title>(title);
                _context.Titles.Remove(title);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Title>.Failure(new[] { ex.Message });
            }
            return Result<Title>.Success(title);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteTitleMapper : Profile
    {
        public DeleteTitleMapper()
        {
            CreateMap<Title, DeleteTitleCommand>().ReverseMap();
        }
    }
#endregion
#endregion
