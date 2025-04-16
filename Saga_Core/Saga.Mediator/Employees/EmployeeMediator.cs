using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Employees;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Entities.Systems;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Employees;
using Saga.DomainShared;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Models;
using Saga.Mediator.Services;
using Saga.Mediator.Systems.AssetMediator;
using Saga.Persistence.Context;
using Saga.Persistence.Models;
using Saga.Validators.Employees;
using System.Linq.Expressions;

namespace Saga.Mediator.Employees.EmployeeMediator;

#region "Get List Employee"
#region "Query"
    public sealed record GetEmployeesQuery(Expression<Func<Employee, bool>>[] wheres) : IRequest<EmployeeList>;
#endregion
#region "Handler"
    public sealed class GetEmployeesQueryHandler(IEmployeeRepository _repo) : IRequestHandler<GetEmployeesQuery, EmployeeList>
    {
        public async Task<EmployeeList> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
            => await _repo.GetAllEmployees(request.wheres);
    }
#endregion
#endregion

#region "Get List Employee With Pagination"
#region "Query"
    public sealed record GetEmployeesPaginationQuery(Expression<Func<Employee, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<EmployeeItemPagination>>;
#endregion
#region "Handler"
    public sealed class GetEmployeesPaginationQueryHandler(IEmployeeRepository _repo) : IRequestHandler<GetEmployeesPaginationQuery, PaginatedList<EmployeeItemPagination>>
    {
        public async Task<PaginatedList<EmployeeItemPagination>> Handle(GetEmployeesPaginationQuery request, CancellationToken cancellationToken)
            => await _repo.GetAllEmployeesWithPagination(request.wheres, request.pagination);
    }
#endregion
#endregion

#region "Get By Id Employee"
#region "Query"
    public sealed record GetEmployeeQuery(Guid Key) : IRequest<EmployeeForm>;
#endregion
#region "Handler"
    public sealed class GetEmployeeQueryHandler(IEmployeeRepository _repo) : IRequestHandler<GetEmployeeQuery, EmployeeForm>
    {
        public async Task<EmployeeForm> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
            => await _repo.GetEmployee(request.Key);
    }
#endregion
#endregion

#region "Get List Direct Supervisor"
#region "Query"
    public sealed record GetDirectSupervisorsQuery(Expression<Func<Employee, bool>>[] wheres) : IRequest<IEnumerable<DirectSupervisorList>>;
#endregion
#region "Handler"
    public sealed class GetDirectSupervisorsQueryHandler(IEmployeeRepository _repo) : IRequestHandler<GetDirectSupervisorsQuery, IEnumerable<DirectSupervisorList>>
    {
        public async Task<IEnumerable<DirectSupervisorList>> Handle(GetDirectSupervisorsQuery request, CancellationToken cancellationToken)
            => await _repo.GetAllDirectSupervisors(request.wheres);
    }
#endregion
#endregion

#region "Save Employee"
#region "Command"
public sealed record SaveEmployeeCommand(EmployeeDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveEmployeeCommandHandler : IRequestHandler<SaveEmployeeCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<EmployeeDto> _EmployeeValidator;
        private readonly IValidator<EmployeePersonalDto> _EmployeePersonalValidator;
        private readonly IValidator<EmployeeEducationDto> _EmployeeEducationValidator;
        private readonly IValidator<EmployeeExperienceDto> _EmployeeExperienceValidator;
        private readonly IValidator<EmployeeFamilyDto> _EmployeeFamilyValidator;
        private readonly IValidator<EmployeeHobbyDto> _EmployeeHobbyValidator;
        private readonly IValidator<EmployeeLanguageDto> _EmployeeLanguageValidator;
        private readonly IValidator<EmployeeSkillDto> _EmployeeSkillValidator;
        private readonly IValidator<EmployeeAttendanceDto> _EmployeeAttendanceValidator;
        private readonly IValidator<EmployeeAttendanceDetailDto> _EmployeeAttendanceDetailValidator;
        private readonly IValidator<EmployeePayrollDto> _EmployeePayrollValidator;
        private readonly IMediator _mediator;
        private readonly IEmployeeRepository _employeeRepository;

        public SaveEmployeeCommandHandler(IDataContext context, 
                                          IValidator<EmployeeDto> EmployeeValidator,
                                          IValidator<EmployeePersonalDto> EmployeePersonalValidator,
                                          IValidator<EmployeeEducationDto> EmployeeEducationValidator,
                                          IValidator<EmployeeExperienceDto> EmployeeExperienceValidator,
                                          IValidator<EmployeeFamilyDto> EmployeeFamilyValidator,
                                          IValidator<EmployeeHobbyDto> EmployeeHobbyValidator,
                                          IValidator<EmployeeLanguageDto> EmployeeLanguageValidator,
                                          IValidator<EmployeeSkillDto> EmployeeSkillValidator,
                                          IValidator<EmployeeAttendanceDto> EmployeeAttendanceValidator,
                                          IValidator<EmployeeAttendanceDetailDto> EmployeeAttendanceDetailValidator,
                                          IValidator<EmployeePayrollDto> EmployeePayrollValidator,
                                          IMediator mediator,
                                          IEmployeeRepository employeeRepository)
        {
            _context = context;
            _EmployeeValidator = EmployeeValidator;
            _EmployeePersonalValidator = EmployeePersonalValidator;
            _EmployeeEducationValidator = EmployeeEducationValidator;
            _EmployeeExperienceValidator = EmployeeExperienceValidator;
            _EmployeeFamilyValidator = EmployeeFamilyValidator;
            _EmployeeHobbyValidator = EmployeeHobbyValidator;
            _EmployeeLanguageValidator = EmployeeLanguageValidator;
            _EmployeeSkillValidator = EmployeeSkillValidator;
            _EmployeeAttendanceValidator = EmployeeAttendanceValidator;
            _EmployeeAttendanceDetailValidator = EmployeeAttendanceDetailValidator;
            _EmployeePayrollValidator = EmployeePayrollValidator;
            _mediator = mediator;
            _employeeRepository = employeeRepository;
        }

        public async Task<Result> Handle(SaveEmployeeCommand command, CancellationToken cancellationToken)
        {
            try
            {
                //Validate Employee
                ValidationResult employeeValidator = await _EmployeeValidator.ValidateAsync(command.Form);
                if (!employeeValidator.IsValid)
                {
                    var failures = employeeValidator.Errors
                                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                            .ToList();
                    return Result.Failure(failures);
                }

                //Upload photo Employee
                if (command.Form.Photo != null && command.Form.Photo.Length > 0)
                {
                    var photoUpload = await _mediator.Send(new UploadFileCommand(command.Form.Photo), cancellationToken);
                    if (!photoUpload.Succeeded)
                        return Result.Failure(photoUpload.Errors);

                    command.Form.AssetKey = photoUpload.Value.Key;
                }

                //Save Employee 
                var employee = command.Form.ConvertToEntity();
                await _employeeRepository.SaveEmployeeAsync(employee, cancellationToken);

                //Save Employee Personal
                if (command.Form.EmployeePersonal != null)
                {
                    command.Form.EmployeePersonal.EmployeeKey = employee.Key;

                    //Check for EmployeePersonal Key to decide RuleSet
                    var personalRuleSet = command.Form.EmployeePersonal.Key == Guid.Empty || command.Form.EmployeePersonal.Key == null ? "Create" : "Update";

                    //Validate Employee Personal
                    ValidationResult employeePersonalValidator = await _EmployeePersonalValidator.ValidateAsync(command.Form.EmployeePersonal, options => options.IncludeRuleSets(personalRuleSet));
                    if (!employeePersonalValidator.IsValid)
                    {
                        var failures = employeePersonalValidator.Errors
                                                .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                                .ToList();
                        return Result.Failure(failures);
                    }

                    var personalEntity = command.Form.EmployeePersonal.ConvertToEntity();
                    await _employeeRepository.SaveEmployeePersonalAsync(personalEntity, cancellationToken);
                }
                
                //Save Employee Educations
                if (command.Form.EmployeeEducations != null && command.Form.EmployeeEducations.Any())
                {
                    foreach (var educationDto in command.Form.EmployeeEducations)
                    {
                        educationDto.EmployeeKey = employee.Key;
                        //Check for EmployeeEducation Key to decide RuleSet
                        var educationRuleSet = educationDto.Key == Guid.Empty || educationDto.Key == null ? "Create" : "Update";
                        
                        ValidationResult employeeEducationValidator = await _EmployeeEducationValidator.ValidateAsync(educationDto, options => options.IncludeRuleSets(educationRuleSet));
                        if (!employeeEducationValidator.IsValid)
                            return Result.Failure(employeeEducationValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    var educationEntities = command.Form.EmployeeEducations.Select(x =>
                    {
                        var entity = x.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.EmployeeKey = employee.Key;
                        return entity;
                    });
                    await _employeeRepository.SaveEmployeeEducationsAsync(employee.Key, educationEntities, cancellationToken);
                }

                //Save Employee Experiences
                if (command.Form.EmployeeExperiences != null && command.Form.EmployeeExperiences.Any())
                {
                    foreach (var experienceDto in command.Form.EmployeeExperiences)
                    {
                        experienceDto.EmployeeKey = employee.Key;
                        //Check for EmployeeExperience Key to decide RuleSet
                        var experienceRuleSet = experienceDto.Key == Guid.Empty || experienceDto.Key == null ? "Create" : "Update";

                        ValidationResult employeeExperienceValidator = await _EmployeeExperienceValidator.ValidateAsync(experienceDto, options => options.IncludeRuleSets(experienceRuleSet));
                        if (!employeeExperienceValidator.IsValid)
                            return Result.Failure(employeeExperienceValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    var experienceEntities = command.Form.EmployeeExperiences.Select(x =>
                    {
                        var entity = x.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.EmployeeKey = employee.Key;
                        return entity;
                    });
                    await _employeeRepository.SaveEmployeeExperiencesAsync(employee.Key, experienceEntities, cancellationToken);
                }

                //Save Employee Families
                if (command.Form.EmployeeFamilies != null && command.Form.EmployeeFamilies.Any())
                {
                    foreach (var familyDto in command.Form.EmployeeFamilies)
                    {
                        familyDto.EmployeeKey = employee.Key;
                        //Check for EmployeeFamily Key to decide RuleSet
                        var familyRuleSet = familyDto.Key == Guid.Empty || familyDto.Key == null ? "Create" : "Update";

                        ValidationResult employeeFamilyValidator = await _EmployeeFamilyValidator.ValidateAsync(familyDto, options => options.IncludeRuleSets(familyRuleSet));
                        if (!employeeFamilyValidator.IsValid)
                            return Result.Failure(employeeFamilyValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    var familyEntities = command.Form.EmployeeFamilies.Select(x =>
                    {
                        var entity = x.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.EmployeeKey = employee.Key;
                        return entity;
                    });
                    await _employeeRepository.SaveEmployeeFamiliesAsync(employee.Key, familyEntities, cancellationToken);
                }

                //Save Employee Hobbies
                if (command.Form.EmployeeHobbies != null && command.Form.EmployeeHobbies.Any())
                {
                    foreach (var hobbyDto in command.Form.EmployeeHobbies)
                    {
                        hobbyDto.EmployeeKey = employee.Key;
                        //Check for EmployeeHobby Key to decide RuleSet
                        var hobbyRuleSet = hobbyDto.Key == Guid.Empty || hobbyDto.Key == null ? "Create" : "Update";

                        ValidationResult employeeHobbyValidator = await _EmployeeHobbyValidator.ValidateAsync(hobbyDto, options => options.IncludeRuleSets(hobbyRuleSet));
                        if (!employeeHobbyValidator.IsValid)
                            return Result.Failure(employeeHobbyValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    var hobbyEntities = command.Form.EmployeeHobbies.Select(x =>
                    {
                        var entity = x.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.EmployeeKey = employee.Key;
                        return entity;
                    });
                    await _employeeRepository.SaveEmployeeHobbiesAsync(employee.Key, hobbyEntities, cancellationToken);
                }

                //Create or Update Employee Languages
                if (command.Form.EmployeeLanguages != null && command.Form.EmployeeLanguages.Any())
                {
                    foreach (var languageDto in command.Form.EmployeeLanguages)
                    {
                        languageDto.EmployeeKey = employee.Key;
                        //Check for EmployeeLanguage Key to decide RuleSet
                        var languageRuleSet = languageDto.Key == Guid.Empty || languageDto.Key == null ? "Create" : "Update";

                        ValidationResult employeeLanguageValidator = await _EmployeeLanguageValidator.ValidateAsync(languageDto, options => options.IncludeRuleSets(languageRuleSet));
                        if (!employeeLanguageValidator.IsValid)
                            return Result.Failure(employeeLanguageValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    var languageEntities = command.Form.EmployeeLanguages.Select(x =>
                    {
                        var entity = x.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.EmployeeKey = employee.Key;
                        return entity;
                    });
                    await _employeeRepository.SaveEmployeeLanguagesAsync(employee.Key, languageEntities, cancellationToken);
                }

                //Create or Update Employee Skills
                if (command.Form.EmployeeSkills != null && command.Form.EmployeeSkills.Any())
                {
                    foreach (var skillDto in command.Form.EmployeeSkills)
                    {
                        skillDto.EmployeeKey = employee.Key;
                        //Check for EmployeeSkill Key to decide RuleSet
                        var skillRuleSet = skillDto.Key == Guid.Empty || skillDto.Key == null ? "Create" : "Update";
                        
                        ValidationResult employeeSkillValidator = await _EmployeeSkillValidator.ValidateAsync(skillDto, options => options.IncludeRuleSets(skillRuleSet));
                        if (!employeeSkillValidator.IsValid)
                            return Result.Failure(employeeSkillValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    var skillEntities = command.Form.EmployeeSkills.Select(x =>
                    {
                        var entity = x.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.EmployeeKey = employee.Key;
                        return entity;
                    });
                    await _employeeRepository.SaveEmployeeSkillsAsync(employee.Key, skillEntities, cancellationToken);
                }

                //Create or Update Employee Attendance With Details
                if (command.Form.EmployeeAttendance != null)
                {
                    command.Form.EmployeeAttendance.EmployeeKey = employee.Key;

                    //Check for EmployeeAttendance Key to decide RuleSet
                    var attendanceRuleSet = command.Form.EmployeeAttendance.Key == Guid.Empty || command.Form.EmployeeAttendance.Key == null ? "Create" : "Update";

                    //Validate Employee Attendance
                    ValidationResult employeeAttendanceValidator = await _EmployeeAttendanceValidator.ValidateAsync(command.Form.EmployeeAttendance, options => options.IncludeRuleSets(attendanceRuleSet));
                    if (!employeeAttendanceValidator.IsValid)
                    {
                        var failures = employeeAttendanceValidator.Errors
                                                .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                                .ToList();
                        return Result.Failure(failures);
                    }

                    var attendanceEntity = command.Form.EmployeeAttendance.ConvertToEntity();
                    await _employeeRepository.SaveEmployeeAttendanceAsync(attendanceEntity, cancellationToken);

                    //Create or Update Employee Attendance Details
                    if (command.Form.EmployeeAttendance.EmployeeAttendanceDetails != null &&
                        command.Form.EmployeeAttendance.EmployeeAttendanceDetails.Any())
                    {
                        foreach (var attendanceDetailDto in command.Form.EmployeeAttendance.EmployeeAttendanceDetails)
                        {
                            attendanceDetailDto.EmployeeAttendanceKey = attendanceEntity.Key;
                            //Check for EmployeeAttendanceDetail Key to decide RuleSet
                            var detailRuleSet = attendanceDetailDto.Key == Guid.Empty || attendanceDetailDto.Key == null ? "Create" : "Update";

                            ValidationResult employeeAttendanceDetailValidator = await _EmployeeAttendanceDetailValidator.ValidateAsync(attendanceDetailDto, options => options.IncludeRuleSets(detailRuleSet));
                            if (!employeeAttendanceDetailValidator.IsValid)
                                return Result.Failure(employeeAttendanceDetailValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                        }

                        var attendanceDetailsEntity = command.Form.EmployeeAttendance.EmployeeAttendanceDetails.Select(sd =>
                        {
                            var entity = sd.ConvertToEntity();
                            entity.Key = Guid.NewGuid();
                            entity.EmployeeAttendanceKey = attendanceEntity.Key;
                            return entity;
                        });

                        await _employeeRepository.SaveEmployeeAttendanceDetailsAsync(attendanceEntity.Key, attendanceDetailsEntity, cancellationToken);
                    }
                }

                //Create or Update Employee Payroll
                if (command.Form.EmployeePayroll != null)
                {
                    command.Form.EmployeePayroll.EmployeeKey = employee.Key;

                    //Check for EmployeePayroll Key to decide RuleSet
                    var payrollRuleSet = command.Form.EmployeePayroll.Key == Guid.Empty || command.Form.EmployeePayroll.Key == null ? "Create" : "Update";

                    //Validate Employee Payroll
                    ValidationResult employeePayrollValidator = await _EmployeePayrollValidator.ValidateAsync(command.Form.EmployeePayroll, options => options.IncludeRuleSets(payrollRuleSet));
                    if (!employeePayrollValidator.IsValid)
                    {
                        var failures = employeePayrollValidator.Errors
                                               .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                               .ToList();
                        return Result.Failure(failures);
                    }

                    var payrollEntity = command.Form.EmployeePayroll.ConvertToEntity();
                    await _employeeRepository.SaveEmployeePayrollAsync(payrollEntity, cancellationToken);
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

#region "Delete Employee"
#region "Command"
    public sealed record DeleteEmployeeCommand(Guid Key) : IRequest<Result<Employee>>;
#endregion
#region "Handler"
    public sealed class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<Employee>>
    {
        private readonly IDataContext _context;
        private readonly IMediator _mediator;

        public DeleteEmployeeCommandHandler(IDataContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Result<Employee>> Handle(DeleteEmployeeCommand command, CancellationToken cancellationToken)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Key == command.Key);

            try
            {
                if (employee == null)
                {
                    throw new Exception("Employee Not Found");
                }

                if (employee.AssetKey != Guid.Empty || employee.AssetKey != null)
                {
                    var photoDelete = await _mediator.Send(new DeleteFileCommand((Guid)employee.AssetKey), cancellationToken);
                    if (!photoDelete.Succeeded)
                        throw new Exception(photoDelete.Errors.FirstOrDefault());
                }

                // Check if EmployeePersonal exists for this Employee
                var employeePersonal = await _context.EmployeePersonals.FirstOrDefaultAsync(ep => ep.EmployeeKey == employee.Key, cancellationToken);
                if (employeePersonal != null)
                {
                    _context.EmployeePersonals.Remove(employeePersonal);
                }

                //Check if any EmployeeEducations exist for this Employee
                var employeeEducations = await _context.EmployeeEducations.Where(ee => ee.EmployeeKey == employee.Key).ToListAsync();
                if (employeeEducations.Any())
                {
                    _context.EmployeeEducations.RemoveRange(employeeEducations);
                }

                //Check if any EmployeeExperiences exist for this Employee
                var employeeExperiences = await _context.EmployeeExperiences.Where(x => x.EmployeeKey == employee.Key).ToListAsync();
                if (employeeExperiences.Any())
                {
                    _context.EmployeeExperiences.RemoveRange(employeeExperiences);
                }

                //Check if any EmployeeFamilies exists for this Employee
                var employeeFamilies = await _context.EmployeeFamilies.Where(e => e.EmployeeKey == employee.Key).ToListAsync();
                if (employeeFamilies.Any())
                {
                    _context.EmployeeFamilies.RemoveRange(employeeFamilies);
                }

                //Check if any EmployeeHobbies exist for this Employee
                var employeeHobbies = await _context.EmployeeHobbies.Where(e => e.EmployeeKey == employee.Key).ToListAsync();
                if (employeeHobbies.Any())
                {
                    _context.EmployeeHobbies.RemoveRange(employeeHobbies);
                }

                //Check if any EmployeeLanguages exist for this Employee
                var employeeLanguages = await _context.EmployeeLanguages.Where(e => e.EmployeeKey == employee.Key).ToListAsync();
                if (employeeLanguages.Any())
                {
                    _context.EmployeeLanguages.RemoveRange(employeeLanguages);
                }

                //Check if any EmployeeSkills exist for this Employee
                var employeeSkills = await _context.EmployeeSkills.Where(e => e.EmployeeKey == employee.Key).ToListAsync();
                if (employeeSkills.Any())
                {
                    _context.EmployeeSkills.RemoveRange(employeeSkills);
                }

                //Check if any Employee Attendance and Details exist for this Employee
                var employeeAttendance = await _context.EmployeesAttendances.FirstOrDefaultAsync(ea => ea.EmployeeKey == employee.Key, cancellationToken);
                if (employeeAttendance != null)
                {
                    //Check if any Employee Attendance Details exist for this Employee Attendance
                    var employeeAttendanceDetails = await _context.EmployeeAttendanceDetails.Where(ead => ead.EmployeeAttendanceKey == employeeAttendance.Key).ToListAsync();
                    if (employeeAttendanceDetails.Any())
                    {
                        _context.EmployeeAttendanceDetails.RemoveRange(employeeAttendanceDetails);
                    }

                    _context.EmployeesAttendances.Remove(employeeAttendance);
                }

                //Check if any Employee Payroll exist for this Employee
                var employeePayroll = await _context.EmployeePayrolls.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key, cancellationToken);
                if (employeePayroll != null)
                {
                    _context.EmployeePayrolls.Remove(employeePayroll);
                }

                _context.Employees.Remove(employee);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Employee>.Failure(new[] { ex.Message });
            }
            return Result<Employee>.Success(employee);
        }
    }
#endregion
#endregion

#region "Get Report Employee Query"
#region "Query"
public sealed record GetEmployeeReportQuery(EmployeeReportDto report, PaginationConfig pagination) : IRequest<EmployeeReport>;
#endregion
#region "Handler"
public sealed class GetEmployeeReportQueryHandler : IRequestHandler<GetEmployeeReportQuery, EmployeeReport>
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeReportQueryHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmployeeReport> Handle(GetEmployeeReportQuery request, CancellationToken cancellationToken)
    {
        var employees = await _repository.GetEmployeeesWithDetails(request.report, request.pagination);

        var viewModel = new EmployeeReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            GradeKey = request.report.GradeKey ?? Guid.Empty,
            Status = request.report.Status ?? null,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            EmployeesData = employees.Items,
            PageNumber = employees.PageNumber,
            TotalCount = employees.TotalCount,
            TotalPages = employees.TotalPages
        };
        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Report Employee"
#region "Query"
public sealed record GenerateEmployeeReportQuery(EmployeeReportDto report, PaginationConfig pagination) : IRequest<byte[]>;
#endregion
#region "Handler"
    public sealed class GenerateEmployeeReportQueryHandler : IRequestHandler<GenerateEmployeeReportQuery, byte[]>
    {
        private readonly IDocumentGenerator _documentGenerator;
        private readonly IEmployeeRepository _repository;

        public GenerateEmployeeReportQueryHandler(IDocumentGenerator documentGenerator, IEmployeeRepository repository)
        {
            _documentGenerator = documentGenerator;
            _repository = repository;
        }

        public async Task<byte[]> Handle(GenerateEmployeeReportQuery request, CancellationToken cancellationToken)
        {
            var employees = await _repository.GetEmployeeesWithDetails(request.report, request.pagination);

            var viewModel = new EmployeeReport
            {
                EmployeesData = employees.Items
            };

            return await _documentGenerator.GenerateDocumentAsync("EmployeeReport", viewModel, request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx);
        }
    }
#endregion
#endregion

#region "Get Curriculum Vitae Report Query Preview"
#region "Query"
    public sealed record GetCurriculumVitaeReportQuery(CurriculumVitaeReportDto report) : IRequest<CurriculumVitaeReport>;
#endregion
#region "Handler"
    public sealed class GetCurriculumVitaeReportHandler(IEmployeeRepository _repo) : IRequestHandler<GetCurriculumVitaeReportQuery, CurriculumVitaeReport>
    {
        public async Task<CurriculumVitaeReport> Handle(GetCurriculumVitaeReportQuery request, CancellationToken cancellationToken)
        {
            var employees = await _repo.GetCurriculumVitaeReportQuery(request.report);

            var viewModel = new CurriculumVitaeReport
            {
                CurriculumVitae = employees,
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Generate Report Curriculum Vitae"
#region "Query"
    public sealed record GenerateCurriculumVitaeReportQuery(CurriculumVitaeReportDto report) : IRequest<byte[]>;
#endregion
#region "Handler"
    public sealed class GenerateCurriculumVitaeReportQueryHandler : IRequestHandler<GenerateCurriculumVitaeReportQuery, byte[]>
    {
        private readonly IEmployeeRepository _repository;
        private readonly IDocumentGenerator _documentGenerator;

        public GenerateCurriculumVitaeReportQueryHandler(IEmployeeRepository repository, IDocumentGenerator documentGenerator)
        {
            _repository = repository;
            _documentGenerator = documentGenerator;
        }

        public async Task<byte[]> Handle(GenerateCurriculumVitaeReportQuery request, CancellationToken cancellationToken)
        {
            var employees = await _repository.GetCurriculumVitaeReportQuery(request.report);

            var curriculumVitaeReport = new CurriculumVitaeReport
            {
                EmployeeKey = request.report.EmployeeKey,
                CompanyKey = request.report.CompanyKey,
                OrganizationKey = request.report.OrganizationKey,
                PositionKey = request.report.PositionKey,
                TitleKey = request.report.TitleKey,
                GradeKey = request.report.GradeKey,
                Status = request.report.Status ?? null,
                DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Pdf,
                CurriculumVitae = employees
            };

            var document = await _documentGenerator.GenerateDocumentAsync("CurriculumVitae", curriculumVitaeReport, request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Pdf);
        
            return document;
        }
    }
#endregion
#endregion

#region "Get Graph Turn Over Query"
#region "Query"
    public sealed record GetGraphTurnOverQuery(GraphTurnOverDto report) : IRequest<GraphTurnOverData>;
#endregion
#region "Handler"
    public sealed class GetGraphTurnOverQueryHandler(IEmployeeRepository _repo) : IRequestHandler<GetGraphTurnOverQuery, GraphTurnOverData>
    {
        public async Task<GraphTurnOverData> Handle(GetGraphTurnOverQuery request, CancellationToken cancellationToken)
            => await _repo.GetGraphTurnOver(request.report);
    }
#endregion
#endregion

#region "Get Graph Man Power Query"
#region "Query"
    public sealed record GetGraphManPowerQuery(GraphManPowerDto report) : IRequest<Dictionary<string, int>>;
#endregion
#region "Handler"
    public sealed class GetGraphManPowerQueryHandler(IEmployeeRepository _repo) : IRequestHandler<GetGraphManPowerQuery, Dictionary<string, int>>
    {
        public Task<Dictionary<string, int>> Handle(GetGraphManPowerQuery request, CancellationToken cancellationToken)
            => _repo.GetGraphManPower(request.report);
    }
#endregion
#endregion

#region "Generate Curriculum Vitae Report From CSHTML"
#region "Query"
    public sealed record GenerateCurriculumVitaeReportCSHTMLQuery(CurriculumVitaeReportDto report) : IRequest<byte[]>;
#endregion
#region "Handler"
    public sealed class GenerateCurriculumVitaeReportCSHTMLQueryHandler : IRequestHandler<GenerateCurriculumVitaeReportCSHTMLQuery, byte[]>
    {
        private readonly IEmployeeRepository _repository;
        private readonly IDocumentGenerator _documentGenerator;

        public GenerateCurriculumVitaeReportCSHTMLQueryHandler(IEmployeeRepository repository, IDocumentGenerator documentGenerator)
        {
            _repository = repository;
            _documentGenerator = documentGenerator;
        }

        public async Task<byte[]> Handle(GenerateCurriculumVitaeReportCSHTMLQuery request, CancellationToken cancellationToken)
        {
            var employees = await _repository.GetCurriculumVitaeReportQuery(request.report);

            var curriculumVitaeReport = new CurriculumVitaeReport
            {
                EmployeeKey = request.report.EmployeeKey,
                CompanyKey = request.report.CompanyKey,
                OrganizationKey = request.report.OrganizationKey,
                PositionKey = request.report.PositionKey,
                TitleKey = request.report.TitleKey,
                GradeKey = request.report.GradeKey,
                Status = request.report.Status ?? null,
                DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Pdf,
                CurriculumVitae = employees
            };

            var htmlTemplate = await _documentGenerator.GetCurriculumVitaeReportHTML(curriculumVitaeReport);

            var document = await _documentGenerator.GenerateCurriculumVitaeReportPDF(htmlTemplate);

            return document;
        }
    }
#endregion
#endregion

#region "Get List Employee Attendance"
#region "Query"
public sealed record GetEmployeeAttendancesQuery(Expression<Func<EmployeeAttendance, bool>>[] wheres) : IRequest<IEnumerable<EmployeeAttendance>>;
#endregion
#region "Handler"
public sealed class GetEmployeeAttendancesQueryHandler : IRequestHandler<GetEmployeeAttendancesQuery, IEnumerable<EmployeeAttendance>>
{
    private readonly IDataContext _context;

    public GetEmployeeAttendancesQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeAttendance>> Handle(GetEmployeeAttendancesQuery request, CancellationToken cancellationToken)
    {
        var queries = from ea in _context.EmployeesAttendances
                      join e in _context.Employees on ea.EmployeeKey equals e.Key
                      join s in _context.Shifts on ea.ShiftKey equals s.Key
                      join sh in _context.ShiftSchedules on ea.ShiftScheduleKey equals sh.Key
                      where !string.IsNullOrEmpty(ea.FingerPrintID)
                              && ea.DeletedAt == null
                      select new
                      {
                          EmployeeAttendance = ea,
                          Employee = e,
                          Shift = s,
                          ShiftSchedule = sh
                      };
        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.EmployeeAttendance));
        }

        var employeeAttendances = await queries.Select(x => new EmployeeAttendance
        {
            Key = x.EmployeeAttendance.Key,
            EmployeeKey = x.EmployeeAttendance.EmployeeKey,
            FingerPrintID = x.EmployeeAttendance.FingerPrintID,
            ShiftKey = x.EmployeeAttendance.ShiftKey,
            ShiftScheduleKey = x.EmployeeAttendance.ShiftScheduleKey,
            Employee = x.Employee,
            Shift = x.Shift,
            ShiftSchedule = x.ShiftSchedule
        }).ToListAsync();

        return employeeAttendances;
    }
}
#endregion
#endregion

#region "Get List Employee Attendance With FingerPrintID"
#region "Query"
public sealed record GetEmployeeAttendancesWithFingerPrintIDQuery(Guid CompanyKey) : IRequest<List<EmployeeAttendance>>;
#endregion
#region "Handler"
    public sealed class GetEmployeeAttendanceWithFingerPrintIDQueryHandler : IRequestHandler<GetEmployeeAttendancesWithFingerPrintIDQuery, List<EmployeeAttendance>>
    {
        private readonly IDataContext _context;

        public GetEmployeeAttendanceWithFingerPrintIDQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeAttendance>> Handle(GetEmployeeAttendancesWithFingerPrintIDQuery request, CancellationToken cancellationToken)
        {
            var employeeAttendances = await(from ea in _context.EmployeesAttendances
                                            join e in _context.Employees on ea.EmployeeKey equals e.Key
                                            join s in _context.Shifts on ea.ShiftKey equals s.Key
                                            join sh in _context.ShiftSchedules on ea.ShiftScheduleKey equals sh.Key
                                            where e.CompanyKey == request.CompanyKey
                                                  && !string.IsNullOrEmpty(ea.FingerPrintID)
                                                  && ea.DeletedAt == null
                                            select new EmployeeAttendance
                                            {
                                                Key = ea.Key,
                                                EmployeeKey = ea.EmployeeKey,
                                                FingerPrintID = ea.FingerPrintID,
                                                ShiftKey = ea.ShiftKey,
                                                ShiftScheduleKey = ea.ShiftScheduleKey,
                                                Employee = e,
                                                Shift = s,
                                                ShiftSchedule = sh
                                            }).ToListAsync();
            return employeeAttendances;
        }
    }
#endregion
#endregion

#region "Get List Employee Attendance With Details"
#region "Query"
public sealed record GetEmployeeAttendanceWithDetailsQuery(Expression<Func<Employee, bool>>[] wheres) : IRequest<IEnumerable<EmployeeAttendance>>;
#endregion
#region "Handler"
public sealed class GetEmployeeAttendanceWithDetailsQueryHandler : IRequestHandler<GetEmployeeAttendanceWithDetailsQuery, IEnumerable<EmployeeAttendance>>
{
    private readonly IDataContext _context;

    public GetEmployeeAttendanceWithDetailsQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeAttendance>> Handle(GetEmployeeAttendanceWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var queries = await (from ea in _context.EmployeesAttendances
                             join emp in _context.Employees on ea.EmployeeKey equals emp.Key
                             join com in _context.Companies on emp.CompanyKey equals com.Key
                             join sh in _context.Shifts on ea.ShiftKey equals sh.Key
                             join sch in _context.ShiftSchedules on ea.ShiftScheduleKey equals sch.Key
                             where ea.DeletedAt == null
                             select new
                             {
                                 EmployeeAttendance = ea,
                                 Shift = sh,
                                 ShiftSchedule = sch,
                                 Employee = emp,
                                 Company = com
                             }).ToListAsync();

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.Employee)).ToList();
        }

        var employeeAttendanceDetails = await (from ead in _context.EmployeeAttendanceDetails
                                               where ead.DeletedAt == null
                                               select new EmployeeAttendanceDetail
                                               {
                                                   Key = ead.Key,
                                                   EmployeeAttendanceKey = ead.EmployeeAttendanceKey,
                                                   Name = ead.Name,
                                                   Quota = ead.Quota,
                                                   Used = ead.Used,
                                                   Credit = ead.Credit,
                                                   ExpiredAt = ead.ExpiredAt,
                                                   Category = ead.Category,
                                                   Priority = ead.Priority
                                               }).ToListAsync();

        var shiftDetails = await (from sd in _context.ShiftDetails
                                  join sh in _context.Shifts on sd.ShiftKey equals sh.Key
                                  where sd.DeletedAt == null
                                  select new ShiftDetail
                                  {
                                      Key = sd.Key,
                                      ShiftKey = sd.ShiftKey,
                                      Day = sd.Day,
                                      WorkName = sd.WorkName,
                                      WorkType = sd.WorkType,
                                      In = sd.In,
                                      Out = sd.Out,
                                      EarlyIn = sd.EarlyIn,
                                      MaxOut = sd.MaxOut,
                                      LateTolerance = sd.LateTolerance,
                                      IsCutBreak = sd.IsCutBreak,
                                      IsNextDay = sd.IsNextDay,
                                      Shift = sh
                                  }).ToListAsync();

        var shiftScheduleDetails = await (from ssd in _context.ShiftScheduleDetails
                                          where ssd.DeletedAt == null
                                          select new ShiftScheduleDetail
                                          {
                                              Key = ssd.Key,
                                              ShiftScheduleKey = ssd.ShiftScheduleKey,
                                              ShiftDetailKey = ssd.ShiftDetailKey,
                                              Date = ssd.Date,
                                              ShiftName = ssd.ShiftName
                                          }).ToListAsync();

        var shiftDetailMap = shiftDetails.ToDictionary(sd => sd.Key, sd => sd);
        foreach (var ssd in shiftScheduleDetails)
        {
            if (shiftDetailMap.TryGetValue(ssd.ShiftDetailKey, out var shiftDetail))
            {
                ssd.ShiftDetail = shiftDetail;
            }
        }

        var employeeAttendances = queries.Select(x => new EmployeeAttendance
        {
            Key = x.EmployeeAttendance.Key,
            EmployeeKey = x.EmployeeAttendance.EmployeeKey,
            FingerPrintID = x.EmployeeAttendance.FingerPrintID,
            ShiftKey = x.EmployeeAttendance.ShiftKey,
            ShiftScheduleKey = x.EmployeeAttendance.ShiftScheduleKey,
            OvertimeMode = x.EmployeeAttendance.OvertimeMode,
            Employee = x.Employee,
            Shift = new Shift
            {
                Key = x.Shift.Key,
                CompanyKey = x.Shift.CompanyKey,
                ShiftGroupName = x.Shift.ShiftGroupName,
                MaxLimit = x.Shift.MaxLimit,
                Description = x.Shift.Description,
                Company = x.Company,
                ShiftDetails = shiftDetails.Where(sd => sd.ShiftKey == x.Shift.Key).ToList()
            },
            ShiftSchedule = new ShiftSchedule
            {
                Key = x.ShiftSchedule.Key,
                CompanyKey = x.ShiftSchedule.CompanyKey,
                GroupName = x.ShiftSchedule.GroupName,
                YearPeriod = x.ShiftSchedule.YearPeriod,
                MonthPeriod = x.ShiftSchedule.MonthPeriod,
                IsRoaster = x.ShiftSchedule.IsRoaster,
                Company = x.Company,
                ShiftScheduleDetails = shiftScheduleDetails.Where(ssd => ssd.ShiftScheduleKey == x.ShiftSchedule.Key).ToList()
            },
            EmployeeAttendanceDetails = employeeAttendanceDetails.Where(d => d.EmployeeAttendanceKey == x.EmployeeAttendance.Key).ToList()
        }).ToList();

        return employeeAttendances;
    }
}
#endregion
#endregion