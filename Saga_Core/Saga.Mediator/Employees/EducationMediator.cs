using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Employees;
using Saga.Domain.Entities.Employees;
using Saga.Domain.ViewModels.Employees;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Employees.EducationMediator;

#region "Get List Education"
#region "Query"
    public sealed record GetEducationsQuery(Expression<Func<Education, bool>>[] wheres) : IRequest<EducationList>;
#endregion
#region "Handler"
    public sealed record GetEducationsQueryHandler : IRequestHandler<GetEducationsQuery, EducationList>
    {
        private readonly IDataContext _context;

        public GetEducationsQueryHandler(IDataContext context)
        {
            _context = context;
        }

    public async Task<EducationList> Handle(GetEducationsQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Educations.AsQueryable().Where(b => b.DeletedAt == null);

        request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });

        var educations = await queries.ToListAsync();

        var viewModel = new EducationList
        {
            Educations = educations.Select(education => education.ConvertToViewModelEducationListItem())
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Get List Education With Pagination"
#region "Query"
    public sealed record GetEducationsPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Education>>;
#endregion
#region "Handler"
    public sealed class GetEducationsPaginationQueryHandler : IRequestHandler<GetEducationsPaginationQuery, PaginatedList<Education>>
    {
        private readonly IDataContext _context;

        public GetEducationsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

    public async Task<PaginatedList<Education>> Handle(GetEducationsPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Educations.AsQueryable().Where(b => b.DeletedAt == null);

        string search = request.pagination.Find;

        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Description, $"%{search}%"));
        }

        var educations = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

        return await Task.FromResult(educations);
    }
}
#endregion
#endregion

#region "Get By Id Education"
#region "Query"
public sealed record GetEducationQuery(Guid Key) : IRequest<EducationForm>;
#endregion
#region "Handler"
    public sealed class GetEducationQueryHandler : IRequestHandler<GetEducationQuery, EducationForm>
    {
        private readonly IDataContext _context;

        public GetEducationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<EducationForm> Handle(GetEducationQuery request, CancellationToken cancellationToken)
        {
            var education = await _context.Educations.FirstOrDefaultAsync(e => e.Key == request.Key);
            if (education == null || education.DeletedAt != null)
            {
                throw new InvalidOperationException("Education not found or has been deleted.");
            }
            return education.ConvertToViewModelEducationForm();
        }
    }
#endregion
#endregion

#region "Save Education"
#region "Command"
public sealed record SaveEducationCommand(EducationDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveEducationCommandHandler : IRequestHandler<SaveEducationCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<EducationDto> _validator;

        public SaveEducationCommandHandler(IDataContext context, IValidator<EducationDto> validator)
        {
            _context = context;
            _validator = validator;
        }

    public async Task<Result> Handle(SaveEducationCommand command, CancellationToken cancellationToken)
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

            var education = command.Form.ConvertToEntity();

            if (education.Key == Guid.Empty)
            {
                education.Key = Guid.NewGuid();
            }

            //Check if education exists
            var existingEducation = await _context.Educations
                                                  .FirstOrDefaultAsync(e => e.Key == education.Key && e.DeletedAt == null, cancellationToken);

            if (existingEducation == null)
            {
                //Add new Education
                _context.Educations.Add(education);
            }
            else
            {
                //Update existing Education
                education.CreatedAt = existingEducation.CreatedAt;
                education.CreatedBy = existingEducation.CreatedBy;
                _context.Educations.Entry(existingEducation).CurrentValues.SetValues(education);
            }

            var result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(new[] { ex.Message });
        }

        return Result.Success();
    }
}
#endregion
#endregion

#region "Delete Education"
#region "Command"
    public sealed record DeleteEducationCommand(Guid Key) : IRequest<Result<Education>>;
#endregion
#region "Handler"
    public sealed class DeleteEducationCommandHandler : IRequestHandler<DeleteEducationCommand, Result<Education>>
    {
        private readonly IDataContext _context;

        public DeleteEducationCommandHandler(IDataContext context)
        {
            _context = context;
        }

    public async Task<Result<Education>> Handle(DeleteEducationCommand command, CancellationToken cancellationToken)
    {
        var education = await _context.Educations.FirstOrDefaultAsync(e => e.Key == command.Key);

        try
        {
            if (education == null)
            {
                throw new Exception("Education Not Found");
            }

            _context.Educations.Remove(education);
            var result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Education>.Failure(new[] { ex.Message });
        }

        return Result<Education>.Success(education);
    }
}
#endregion
#endregion
