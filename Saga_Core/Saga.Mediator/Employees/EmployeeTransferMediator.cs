using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Employees;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Employees;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Employees.EmployeeTransferMediator;

#region "Get List Employee Transfer"
#region "Query"
    public sealed record GetEmployeeTransfersQuery(Expression<Func<EmployeeTransfer, bool>>[] wheres) : IRequest<EmployeeTransferList>;
#endregion
#region "Handler"
    public sealed class GetEmployeeTransfersQueryHandler : IRequestHandler<GetEmployeeTransfersQuery, EmployeeTransferList>
    {
        private readonly IDataContext _context;

        public GetEmployeeTransfersQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<EmployeeTransferList> Handle(GetEmployeeTransfersQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.EmployeeTransfers.AsQueryable().Where(b => b.DeletedAt == null);

            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });

            var employeeTransfers = await queries.ToListAsync();

            employeeTransfers.ForEach(x =>
            {
                x.Employee = _context.Employees.FirstOrDefault(f => f.Key == x.EmployeeKey);
            });

            var viewModel = new EmployeeTransferList
            {
                EmployeeTransfers = employeeTransfers.Select(x => x.ConvertToViewModelEmployeeTransferItemList())
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Employee Transfer With Pagination"
#region "Query"
    public sealed record GetEmployeeTransfersPaginationQuery(Expression<Func<EmployeeTransfer, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<EmployeeTransferItemPaginationList>>;
#endregion
#region "Handler"
    public sealed class GetEmployeeTransfersPaginationQueryHandler : IRequestHandler<GetEmployeeTransfersPaginationQuery, PaginatedList<EmployeeTransferItemPaginationList>>
    {
        private readonly IDataContext _context;

        public GetEmployeeTransfersPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<EmployeeTransferItemPaginationList>> Handle(GetEmployeeTransfersPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from et in _context.EmployeeTransfers
                          join e in _context.Employees on et.EmployeeKey equals e.Key
                          where et.DeletedAt == null
                          select new EmployeeTransfer
                          {
                              Key = et.Key,
                              EmployeeKey = et.EmployeeKey,
                              TransferCategory = et.TransferCategory,
                              CreatedAt = et.CreatedAt,
                              EffectiveDate = et.EffectiveDate,
                              TransferStatus = et.TransferStatus,
                              CancelledReason = et.CancelledReason,
                              Employee = e
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Employee.Code, $"%{search}%") || EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") || EF.Functions.ILike(b.Employee.LastName, $"%{search}%") || EF.Functions.ILike(b.TransferCategory.ToString(), $"%{search}%"));
            }

            // Applying additional filters from 'wheres'
            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var result = await queries
            .Select(x => new EmployeeTransferItemPaginationList
            {
                EmployeeTransferKey = x.Key,
                EmployeeKey = x.EmployeeKey,
                EmployeeName = (x.Employee.FirstName ?? String.Empty) + " " + (x.Employee.LastName ?? String.Empty),
                EmployeeCode = x.Employee.Code ?? String.Empty,
                TransferCategory = x.TransferCategory.ToString(),
                CreatedDate = x.CreatedAt,
                EffectiveDate = x.EffectiveDate,
                TransferStatus = x.TransferStatus.ToString(),
                IsSelected = false,
                CancelledReason = x.CancelledReason ?? String.Empty,
                // Add computed properties
                CanBeCancelled = x.TransferStatus == Domain.Enums.TransferStatus.Submitted,
                IsCheckboxVisible = x.TransferStatus == Domain.Enums.TransferStatus.Submitted,
                IsRowEnabled = x.TransferStatus == Domain.Enums.TransferStatus.Submitted
            })
            .PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(result);
        }
    }
#endregion
#endregion

#region "Get By Id Employee Transfer"
#region "Query"
    public sealed record GetEmployeeTransferQuery(Guid Key) : IRequest<EmployeeTransferForm>;
#endregion
#region "Handler"
    public sealed class GetEmployeeTransferQueryHandler : IRequestHandler<GetEmployeeTransferQuery, EmployeeTransferForm>
    {
        private readonly IDataContext _context;

        public GetEmployeeTransferQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<EmployeeTransferForm> Handle(GetEmployeeTransferQuery request, CancellationToken cancellationToken)
        {
            var employeeTransfer = await (from emt in _context.EmployeeTransfers
                                          join emp in _context.Employees on emt.EmployeeKey equals emp.Key
                                          join personal in _context.EmployeePersonals on emp.Key equals personal.EmployeeKey
                                          join oldCompany in _context.Companies on emt.OldCompanyKey equals oldCompany.Key
                                          join oldOrganization in _context.Organizations on emt.OldOrganizationKey equals oldOrganization.Key
                                          join oldPosition in _context.Positions on emt.OldPositionKey equals oldPosition.Key
                                          join oldTitle in _context.Titles on emt.OldTitleKey equals oldTitle.Key
                                          join oldBranch in _context.Branches on emt.OldBranchKey equals oldBranch.Key
                                          join newCompany in _context.Companies on emt.NewCompanyKey equals newCompany.Key
                                          join newOrganization in _context.Organizations on emt.NewOrganizationKey equals newOrganization.Key
                                          join newPosition in _context.Positions on emt.NewPositionKey equals newPosition.Key
                                          join newTitle in _context.Titles on emt.NewTitleKey equals newTitle.Key
                                          join newBranch in _context.Branches on emt.NewBranchKey equals newBranch.Key
                                          join dr in _context.Employees on emp.DirectSupervisorKey equals dr.Key into parentSupervisor
                                          from dr in parentSupervisor.DefaultIfEmpty()
                                          where emt.Key == request.Key
                                          select new EmployeeTransfer
                                          {
                                              Key = emt.Key,
                                              EmployeeKey = emt.EmployeeKey,
                                              TransferCategory = emt.TransferCategory,
                                              EffectiveDate = emt.EffectiveDate,
                                              OldCompanyKey = emt.OldCompanyKey,
                                              OldOrganizationKey = emt.OldOrganizationKey,
                                              OldPositionKey = emt.OldPositionKey,
                                              OldTitleKey = emt.OldTitleKey,
                                              OldBranchKey = emt.OldBranchKey,
                                              NewCompanyKey = emt.NewCompanyKey,
                                              NewOrganizationKey = emt.NewOrganizationKey,
                                              NewPositionKey = emt.NewPositionKey,
                                              NewTitleKey = emt.NewTitleKey,
                                              NewBranchKey = emt.NewBranchKey,
                                              TransferStatus = emt.TransferStatus,
                                              Employee = emp,
                                              OldCompany = oldCompany,
                                              OldOrganization = oldOrganization,
                                              OldPosition = oldPosition,
                                              OldTitle = oldTitle,
                                              OldBranch = oldBranch,
                                              NewCompany = newCompany,
                                              NewOrganization = newOrganization,
                                              NewPosition = newPosition,
                                              NewTitle = newTitle,
                                              NewBranch = newBranch,
                                          }).FirstOrDefaultAsync();

            if (employeeTransfer == null)
                throw new Exception("Employee Transfer Not Found");

            var employeeTransferForm = employeeTransfer.ConvertToViewModelEmployeeTransferForm();

            return employeeTransferForm;
        }
    }
#endregion
#endregion

#region "Get Detail Populate Employee"
#region "Query"
    public sealed record GetDetailPopulateEmployeeQuery(Guid EmployeeKey) : IRequest<DetailPopulateEmployee>;
#endregion
#region "Handler"
    public sealed class GetDetailPopulateEmployeeQueryHandler : IRequestHandler<GetDetailPopulateEmployeeQuery, DetailPopulateEmployee>
    {
        private readonly IDataContext _context;

        public GetDetailPopulateEmployeeQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<DetailPopulateEmployee> Handle(GetDetailPopulateEmployeeQuery request, CancellationToken cancellationToken)
        {
            var employee = await (from e in _context.Employees
                                  join com in _context.Companies on e.CompanyKey equals com.Key
                                  join org in _context.Organizations on e.OrganizationKey equals org.Key
                                  join pos in _context.Positions on e.PositionKey equals pos.Key
                                  join ti in _context.Titles on e.TitleKey equals ti.Key
                                  join br in _context.Branches on e.BranchKey equals br.Key
                                  join gr in _context.Grades on e.GradeKey equals gr.Key
                                  join dr in _context.Employees on e.DirectSupervisorKey equals dr.Key into parentSupervisor
                                  from dr in parentSupervisor.DefaultIfEmpty()
                                  join p in _context.EmployeePersonals on e.Key equals p.EmployeeKey into employeePersonalGroup
                                  from ep in employeePersonalGroup.DefaultIfEmpty()
                                  where e.Key == request.EmployeeKey && e.DeletedAt == null
                                  select new Employee
                                  {
                                      Key = e.Key,
                                      FirstName = e.FirstName,
                                      LastName = e.LastName,
                                      Code = e.Code,
                                      Company = com,
                                      Organization = org,
                                      Position = pos,
                                      Title = ti,
                                      Branch = br,
                                      Grade = gr,
                                      DirectSupervisor = dr,
                                      EmployeePersonal = ep
                                  }).FirstOrDefaultAsync();

            if (employee == null) 
                throw new Exception("Employee Not Found");

            var result = employee.ConvertToViewModelDetailPopulateEmployee();

            return result;
        }
    }
#endregion
#endregion

#region "Save Employee Transfer"
#region "Command"
    public sealed record SaveEmployeeTransferCommand(EmployeeTransferDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveEmployeeTransferCommandHandler : IRequestHandler<SaveEmployeeTransferCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<EmployeeTransferDto> _validator;

        public SaveEmployeeTransferCommandHandler(IDataContext context, IValidator<EmployeeTransferDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveEmployeeTransferCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Key == command.Form.EmployeeKey);
                if (employee == null)
                    throw new Exception("Employee Not Found");

                ValidationResult validator = await _validator.ValidateAsync(command.Form);
                if (!validator.IsValid)
                {
                    var failures = validator.Errors
                                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                            .ToList();
                    return Result.Failure(failures);
                }

                var employeeTransfer = command.Form.ConvertToEntity();

                if (employeeTransfer.Key == Guid.Empty)
                {
                    // Create Employee Transfer
                    employeeTransfer.Key = Guid.NewGuid();
                    employeeTransfer.TransferStatus = TransferStatus.Draft;
                }
                else
                {
                    // Update Employee Transfer
                    // Only allow status changes from Draft to Submitted
                    if (command.Form.TransferStatus == TransferStatus.Submitted &&
                        employeeTransfer.TransferStatus == TransferStatus.Draft)
                    {
                        employeeTransfer.TransferStatus = TransferStatus.Submitted;
                    }
                    // Or from Submitted to Canceled
                    else if (employeeTransfer.TransferStatus == TransferStatus.Submitted &&
                             command.Form.TransferStatus == TransferStatus.Canceled)
                    {
                        employeeTransfer.TransferStatus = (TransferStatus)command.Form.TransferStatus;
                    }
                }

                //Check if employee transfer exists
                var existingEmployeeTransfer = await _context.EmployeeTransfers.FirstOrDefaultAsync(x => x.Key == command.Form.Key);

                employeeTransfer.OldCompanyKey = employee.CompanyKey;
                employeeTransfer.OldOrganizationKey = employee.OrganizationKey;
                employeeTransfer.OldPositionKey = employee.PositionKey;
                employeeTransfer.OldTitleKey = employee.TitleKey;
                employeeTransfer.OldBranchKey = employee.BranchKey;

                if (existingEmployeeTransfer == null)
                {
                    //Add new employee transfer
                    _context.EmployeeTransfers.Add(employeeTransfer);
                }
                else
                {
                    //Update existing employee transfer
                    employeeTransfer.CreatedAt = existingEmployeeTransfer.CreatedAt;
                    employeeTransfer.CreatedBy = existingEmployeeTransfer.CreatedBy;
                    _context.EmployeeTransfers.Entry(existingEmployeeTransfer).CurrentValues.SetValues(employeeTransfer);
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

#region "Delete Employee Transfer"
#region "Command"
    public sealed record DeleteEmployeeTransferCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class DeleteEmployeeTransferCommandHandler : IRequestHandler<DeleteEmployeeTransferCommand, Result>
    {
        private readonly IDataContext _context;

        public DeleteEmployeeTransferCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteEmployeeTransferCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var employeeTransfer = await _context.EmployeeTransfers.FirstOrDefaultAsync(x => x.Key == command.Key);
                if (employeeTransfer == null)
                    throw new InvalidOperationException("Employee Transfer not found.");

                _context.EmployeeTransfers.Remove(employeeTransfer);
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

#region "Cancel Employee Transfer"
#region "Command"
    public sealed record CancelEmployeeTransferCommand(CancelEmployeeTransferDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class CancelEmployeeTransferCommandHandler : IRequestHandler<CancelEmployeeTransferCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<CancelEmployeeTransferDto> _validator;

        public CancelEmployeeTransferCommandHandler(IDataContext context, IValidator<CancelEmployeeTransferDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(CancelEmployeeTransferCommand command, CancellationToken cancellationToken)
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

                var employeeTransfers = await _context.EmployeeTransfers.Where(x => command.Form.EmployeeTransferKeys.Contains(x.Key)).ToListAsync();
                if (!employeeTransfers.Any())
                    throw new InvalidOperationException("No valid employee transfers found to cancel.");

                // Validate that all transfers are in a cancellable state
                var nonPendingTransfers = employeeTransfers
                    .Where(x => x.TransferStatus != Domain.Enums.TransferStatus.Submitted)
                    .Select(x => x.Key)
                    .ToList();

                if (nonPendingTransfers.Any())
                {
                    return Result.Failure(new[] { "Some transfers cannot be cancelled because they are not in Pending status." });
                }

                foreach (var employeeTransfer in employeeTransfers)
                {
                    employeeTransfer.TransferStatus = Domain.Enums.TransferStatus.Canceled;
                    employeeTransfer.CancelledReason = command.Form.CancelledReason;
                }
                _context.EmployeeTransfers.UpdateRange(employeeTransfers);
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

#region "Get List Pending Employee Transfer"
#region "Query"
    public sealed record GetPendingEmployeeTransfersQuery(DateOnly effectiveDate) : IRequest<List<EmployeeTransfer>>;
#endregion
#region "Handler"
    public sealed class GetPendingEmployeeTransfersQueryHandler : IRequestHandler<GetPendingEmployeeTransfersQuery, List<EmployeeTransfer>>
    {
        private readonly IDataContext _context;

        public GetPendingEmployeeTransfersQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeTransfer>> Handle(GetPendingEmployeeTransfersQuery request, CancellationToken cancellationToken)
        {
            var pendingTransfers = await _context.EmployeeTransfers
                                             .Where(x => x.TransferStatus == TransferStatus.Submitted
                                                    && x.EffectiveDate.HasValue
                                                    && x.EffectiveDate.Value == request.effectiveDate
                                                    && (!x.IsProcessed.HasValue || x.IsProcessed.Value == false))
                                             .ToListAsync();

            return pendingTransfers;
        }
    }
#endregion
#endregion

#region "Process Employee Transfer"
#region "Command"
    public sealed record ProcessEmployeeTransferCommand(EmployeeTransfer transfer) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class ProcessEmployeeTransferCommandHandler : IRequestHandler<ProcessEmployeeTransferCommand, Result>
    {
        private readonly IDataContext _context;

        public ProcessEmployeeTransferCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(ProcessEmployeeTransferCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.transfer != null)
                { 
                    var employee = await _context.Employees
                                                 .FirstOrDefaultAsync(x => x.Key == command.transfer.EmployeeKey, cancellationToken);
                    if (employee != null)
                    {
                        employee.CompanyKey = command.transfer.NewCompanyKey;
                        employee.OrganizationKey = command.transfer.NewOrganizationKey;
                        employee.PositionKey = command.transfer.NewPositionKey;
                        employee.TitleKey = command.transfer.NewTitleKey;
                        employee.BranchKey = command.transfer.NewBranchKey;

                        _context.Employees.Update(employee);
                    }

                    var existingEmployeeTransfer = await _context.EmployeeTransfers
                                                                .FirstOrDefaultAsync(x => x.Key == command.transfer.Key, cancellationToken);
                    if (existingEmployeeTransfer != null)
                    {
                        existingEmployeeTransfer.TransferStatus = TransferStatus.Transferred;
                        existingEmployeeTransfer.IsProcessed = true;

                        _context.EmployeeTransfers.Update(existingEmployeeTransfer);
                    }

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
#endregion
