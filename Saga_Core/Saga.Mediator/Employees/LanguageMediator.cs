using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Employees;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.ViewModels.Employees;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Employees.LanguageMediator;

#region "Get List Language"
#region "Query"
    public sealed record GetLanguagesQuery(Expression<Func<Language, bool>>[] wheres) : IRequest<LanguageList>;
#endregion
#region "Handler"
    public sealed class GetLanguagesQueryHandler : IRequestHandler<GetLanguagesQuery, LanguageList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetLanguagesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LanguageList> Handle(GetLanguagesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Languages.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });
            var languages = await queries.ToListAsync();

            var viewModel = new LanguageList
            {
                Languages = _mapper.Map<IEnumerable<Language>>(languages)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetLanguagesMapper : Profile
    {
        public GetLanguagesMapper()
        {
            CreateMap<Language, LanguageList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Language With Pagination"
#region "Query"
    public sealed record GetLanguagesPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Language>>;
#endregion
#region "Handler"
    public sealed class GetLanguagesPaginationQueryHandler : IRequestHandler<GetLanguagesPaginationQuery, PaginatedList<Language>>
    {
        private readonly IDataContext _context;

        public GetLanguagesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Language>> Handle(GetLanguagesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Languages.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Description, $"%{search}%"));
            }
        
            var languages = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(languages);
        }
    }
#endregion
#endregion

#region "Get By Id Language"
#region "Query"
    public sealed record GetLanguageQuery(Guid Key) : IRequest<LanguageForm>;
#endregion
#region "Handler"
    public sealed class GetLanguageQueryHandler : IRequestHandler<GetLanguageQuery, LanguageForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetLanguageQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LanguageForm> Handle(GetLanguageQuery request, CancellationToken cancellationToken)
        {
            var language = await _context.Languages.FirstOrDefaultAsync(b => b.Key == request.Key);
            if (language == null || language.DeletedAt != null)
            {
                throw new InvalidOperationException("Language not found or has been deleted.");
            }
            return _mapper.Map<LanguageForm>(language);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetLanguageMapper : Profile
    {
        public GetLanguageMapper()
        {
            CreateMap<Language, LanguageForm>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Save Language"
#region "Command"
    public sealed record SaveLanguageCommand(LanguageDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveLanguageHandler : IRequestHandler<SaveLanguageCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<LanguageDto> _validator;

        public SaveLanguageHandler(IDataContext context, IMapper mapper, IValidator<LanguageDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }
        public async Task<Result> Handle(SaveLanguageCommand command, CancellationToken cancellationToken)
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
                    //Create Language
                    var language = _mapper.Map<Language>(command.Form);
                    language.Key = Guid.NewGuid();

                    _context.Languages.Add(language);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var language = _mapper.Map<Language>(command.Form);
                    if (language == null)
                    {
                        throw new Exception("Language Not Found");
                    }
                    _context.Languages.Update(language);
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
    public sealed class SaveLanguageMapper : Profile
    {
        public SaveLanguageMapper()
        {
            CreateMap<Language, LanguageDto>().ReverseMap();
            CreateMap<Language, SaveLanguageCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Language"
#region "Command"
    public sealed record DeleteLanguageCommand(Guid Key) : IRequest<Result<Language>>;
#endregion
#region "Handler"
    public sealed class DeleteLanguageCommandHandler : IRequestHandler<DeleteLanguageCommand, Result<Language>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteLanguageCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Language>> Handle(DeleteLanguageCommand command, CancellationToken cancellationToken)
        {
            var language = await _context.Languages.FirstOrDefaultAsync(b => b.Key == command.Key);

            try
            {
                if (language == null)
                {
                    throw new Exception("Language Not Found");
                }
                _mapper.Map<Language>(command);
                _context.Languages.Remove(language);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Language>.Failure(new[] { ex.Message });
            }
            return Result<Language>.Success(language);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteLanguageMapper : Profile
    {
        public DeleteLanguageMapper()
        {
            CreateMap<Language, DeleteLanguageCommand>().ReverseMap();
        }
    }
#endregion
#endregion
