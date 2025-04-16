using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Saga.Domain.Dtos.Employees;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Entities.Systems;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;
using Saga.Domain.ViewModels.Employees;
using Saga.Domain.ViewModels.Systems;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Globalization;
using System.Linq.Expressions;

namespace Saga.Mediator.Services;

public interface IEmployeeRepository
{
    Task<EmployeeList> GetAllEmployees(Expression<Func<Employee, bool>>[] wheres);
    Task<PaginatedList<EmployeeItemPagination>> GetAllEmployeesWithPagination(Expression<Func<Employee, bool>>[] wheres, PaginationConfig pagination);
    Task<EmployeeForm> GetEmployee(Guid Key);
    Task<IEnumerable<DirectSupervisorList>> GetAllDirectSupervisors(Expression<Func<Employee, bool>>[] wheres);
    Task SaveEmployeeAsync(Employee employee, CancellationToken cancellationToken);
    Task SaveEmployeePersonalAsync(EmployeePersonal employeePersonal, CancellationToken cancellationToken);
    Task SaveEmployeeEducationsAsync(Guid employeeKey, IEnumerable<EmployeeEducation> educations, CancellationToken cancellationToken);
    Task SaveEmployeeExperiencesAsync(Guid employeeKey, IEnumerable<EmployeeExperience> experiences, CancellationToken cancellationToken);
    Task SaveEmployeeFamiliesAsync(Guid employeeKey, IEnumerable<EmployeeFamily> families, CancellationToken cancellationToken);
    Task SaveEmployeeHobbiesAsync(Guid employeeKey, IEnumerable<EmployeeHobby> hobbies, CancellationToken cancellationToken);
    Task SaveEmployeeLanguagesAsync(Guid employeeKey, IEnumerable<EmployeeLanguage> languages, CancellationToken cancellationToken);
    Task SaveEmployeeSkillsAsync(Guid employeeKey, IEnumerable<EmployeeSkill> skills, CancellationToken cancellationToken);
    Task<IEnumerable<EmployeeForm>> GetCurriculumVitaeReportQuery(CurriculumVitaeReportDto report);
    Task LoadEmployeePhotos(IEnumerable<EmployeeForm> employees);
    Task<GraphTurnOverData> GetGraphTurnOver(GraphTurnOverDto report);
    Task<PaginatedList<EmployeeForm>> GetEmployeeesWithDetails(GeneralEmployeeReportDto report, PaginationConfig pagination);
    Task<Dictionary<string, int>> GetGraphManPower(GraphManPowerDto report);
    Task SaveEmployeeAttendanceAsync(EmployeeAttendance attendance, CancellationToken cancellationToken);
    Task SaveEmployeeAttendanceDetailsAsync(Guid attendanceKey, IEnumerable<EmployeeAttendanceDetail> details, CancellationToken cancellationToken);
    Task SaveEmployeePayrollAsync(EmployeePayroll employeePayroll, CancellationToken cancellationToken);
    Task<IEnumerable<LeaveQuota>> GetEmployeeLeaveQuotas(Guid employeeKey);
    Task<IEnumerable<ApprovalStatusItemList>> GetEmployeeApprovalStatuses(Guid employeeKey);
    Task<IEnumerable<Employee>> GetEmployeesByHierarchy(Guid? companyKey, Guid? organizationKey, Guid? positionKey, Guid? titleKey);
}

public class EmployeeRepository(IDataContext _context, IWebHostEnvironment _webHostEnvironment) : IEmployeeRepository
{
    public async Task<EmployeeList> GetAllEmployees(Expression<Func<Employee, bool>>[] wheres)
    {

        // Start with the base query with the DeletedAt condition
        var employeeQuery = _context.Employees.Where(emp => emp.DeletedAt == null);

        // Apply each filter to the employee query
        foreach (var filter in wheres)
        {
            employeeQuery = employeeQuery.Where(filter);
        }

        var queries = from emp in employeeQuery
                      join personal in _context.EmployeePersonals on emp.Key equals personal.EmployeeKey
                      join company in _context.Companies on emp.CompanyKey equals company.Key
                      join organization in _context.Organizations on emp.OrganizationKey equals organization.Key
                      join position in _context.Positions on emp.PositionKey equals position.Key
                      join title in _context.Titles on emp.TitleKey equals title.Key
                      join branch in _context.Branches on emp.BranchKey equals branch.Key
                      join grade in _context.Grades on emp.GradeKey equals grade.Key
                      join dr in _context.Employees on emp.DirectSupervisorKey equals dr.Key into parentSupervisor
                      from dr in parentSupervisor.DefaultIfEmpty()
                      join py in _context.EmployeePayrolls on emp.Key equals py.EmployeeKey into parentPayroll
                      from payroll in parentPayroll.DefaultIfEmpty()
                      //where emp.DeletedAt == null
                      select new
                      {
                          Employee = emp,
                          EmployeePersonal = personal,
                          Company = company,
                          Organization = organization,
                          Position = position,
                          Title = title,
                          Branch = branch,
                          Grade = grade,
                          DirectSupervisor = dr,
                          EmployeePayroll = payroll
                      };
        //foreach (var filter in wheres)
        //{
        //    queries = queries.Where(x => filter.Compile().Invoke(x.Employee));
        //}

        var employees = await queries.ToListAsync();
        var viewModel = new EmployeeList
        {
            Employees = employees.Select(emp => new Employee
            {
                Key = emp.Employee.Key,
                Code = emp.Employee.Code,
                FirstName = emp.Employee.FirstName,
                LastName = emp.Employee.LastName,
                AssetKey = emp.Employee.AssetKey,
                CompanyKey = emp.Employee.CompanyKey,
                OrganizationKey = emp.Employee.OrganizationKey,
                PositionKey = emp.Employee.PositionKey,
                TitleKey = emp.Employee.TitleKey,
                BranchKey = emp.Employee.BranchKey,
                GradeKey = emp.Employee.GradeKey,
                HireDate = emp.Employee.HireDate,
                Status = emp.Employee.Status,
                DirectSupervisorKey = emp.Employee.DirectSupervisorKey ?? Guid.Empty,
                Company = emp.Company,
                Organization = emp.Organization,
                Position = emp.Position,
                Title = emp.Title,
                Branch = emp.Branch,
                Grade = emp.Grade,
                DirectSupervisor = emp.DirectSupervisor ?? null,
                EmployeePersonal = emp.EmployeePersonal,
                EmployeePayroll = emp.EmployeePayroll ?? null
            })
        };

        return await Task.FromResult(viewModel);
    }

    public async Task<PaginatedList<EmployeeItemPagination>> GetAllEmployeesWithPagination(Expression<Func<Employee, bool>>[] wheres, PaginationConfig pagination)
    {
        var queries = from emp in _context.Employees
                      join personal in _context.EmployeePersonals on emp.Key equals personal.EmployeeKey
                      join company in _context.Companies on emp.CompanyKey equals company.Key
                      join organization in _context.Organizations on emp.OrganizationKey equals organization.Key
                      join position in _context.Positions on emp.PositionKey equals position.Key
                      join title in _context.Titles on emp.TitleKey equals title.Key
                      join branch in _context.Branches on emp.BranchKey equals branch.Key
                      join grade in _context.Grades on emp.GradeKey equals grade.Key
                      join dr in _context.Employees on emp.DirectSupervisorKey equals dr.Key into parentSupervisor
                      from dr in parentSupervisor.DefaultIfEmpty()
                      where emp.DeletedAt == null
                      select new
                      {
                          Employee = emp,
                          EmployeePersonal = personal,
                          Company = company,
                          Organization = organization,
                          Position = position,
                          Title = title,
                          Branch = branch,
                          Grade = grade,
                          DirectSupervisor = dr
                      };
        string search = pagination.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Employee.Code, $"%{search}%") || EF.Functions.ILike(b.EmployeePersonal.NationalityNumber, $"%{search}%") || EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") || EF.Functions.ILike(b.Employee.LastName, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%") || EF.Functions.ILike(b.Position.Name, $"%{search}%"));
        }

        // Applying additional filters from 'wheres'
        foreach (var filter in wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.Employee));
        }

        var employees = await queries.Select(x => new EmployeeItemPagination
        {
            Employee = x.Employee,
            Age = DateTimeDuration.GetAge(x.EmployeePersonal),
            LongOfJoin = DateTimeDuration.GetLongOfJoin(x.Employee)
        })
        .PaginatedListAsync(pagination.PageNumber, pagination.PageSize);

        employees.Items.ForEach(x => {
            x.Company = _context.Companies.FirstOrDefault(f => f.Key == x.Employee.CompanyKey);
            x.Organization = _context.Organizations.FirstOrDefault(f => f.Key == x.Employee.OrganizationKey);
            x.Position = _context.Positions.FirstOrDefault(f => f.Key == x.Employee.PositionKey);
            x.Title = _context.Titles.FirstOrDefault(f => f.Key == x.Employee.TitleKey);
            x.Branch = _context.Branches.FirstOrDefault(f => f.Key == x.Employee.BranchKey);
            x.Grade = _context.Grades.FirstOrDefault(f => f.Key == x.Employee.GradeKey);
            x.DirectSupervisor = _context.Employees.FirstOrDefault(f => f.Key == x.Employee.DirectSupervisorKey);
        });

        return await Task.FromResult(employees);
    }

    public async Task<EmployeeForm> GetEmployee(Guid Key)
    {
        var employee = await(from e in _context.Employees
                             join com in _context.Companies on e.CompanyKey equals com.Key
                             join org in _context.Organizations on e.OrganizationKey equals org.Key
                             join pos in _context.Positions on e.PositionKey equals pos.Key
                             join ti in _context.Titles on e.TitleKey equals ti.Key
                             join br in _context.Branches on e.BranchKey equals br.Key
                             join gr in _context.Grades on e.GradeKey equals gr.Key
                             join dr in _context.Employees on e.DirectSupervisorKey equals dr.Key into parentSupervisor
                             from dr in parentSupervisor.DefaultIfEmpty()
                             join p in _context.EmployeePersonals on e.Key equals p.EmployeeKey into employeePersonalGroup
                             from employeePersonal in employeePersonalGroup.DefaultIfEmpty()
                             join rg in _context.Religions on employeePersonal.ReligionKey equals rg.Key into personalReligionGroup
                             from personalReligion in personalReligionGroup.DefaultIfEmpty()
                             join cou in _context.Countries on employeePersonal.CountryKey equals cou.Key into personalCountryGroup
                             from personalCountry in personalCountryGroup.DefaultIfEmpty()
                             join prov in _context.Provinces on employeePersonal.ProvinceKey equals prov.Key into personalProvinceGroup
                             from personalProvince in personalProvinceGroup.DefaultIfEmpty()
                             join cty in _context.Cities on employeePersonal.CityKey equals cty.Key into personalCityGroup
                             from personalCity in personalCityGroup.DefaultIfEmpty()
                             join ccou in _context.Countries on employeePersonal.CurrentCountryKey equals ccou.Key into personalCurrentCountryGroup
                             from personalCurrentCountry in personalCurrentCountryGroup.DefaultIfEmpty()
                             join cprov in _context.Provinces on employeePersonal.CurrentProvinceKey equals cprov.Key into personalCurrentProvinceGroup
                             from personalCurrentProvince in personalCurrentProvinceGroup.DefaultIfEmpty()
                             join ccty in _context.Cities on employeePersonal.CurrentCityKey equals ccty.Key into personalCurrentCityGroup
                             from personalCurrentCity in personalCurrentCityGroup.DefaultIfEmpty()
                             join etc in _context.Ethnics on employeePersonal.EthnicKey equals etc.Key into personalEthnicGroup
                             from personalEthnic in personalEthnicGroup.DefaultIfEmpty()
                             join nt in _context.Nationalities on employeePersonal.NationalityKey equals nt.Key into personalNationalityGroup
                             from personalNationality in personalNationalityGroup.DefaultIfEmpty() 
                             where e.Key == Key
                             select new
                             {
                                 Employee = e,
                                 Company = com,
                                 Organization = org,
                                 Position = pos,
                                 Title = ti,
                                 Branch = br,
                                 Grade = gr,
                                 DirectSupervisor = dr,
                                 EmployeePersonal = employeePersonal,
                                 PersonalReligion = personalReligion,
                                 PersonalCountry = personalCountry,
                                 PersonalProvince = personalProvince,
                                 PersonalCity = personalCity,
                                 PersonalCurrentCountry = personalCurrentCountry,
                                 PersonalCurrentProvince = personalCurrentProvince,
                                 PersonalCurrentCity = personalCurrentCity,
                                 PersonalEthnic = personalEthnic,
                                 PersonalNationality = personalNationality,
                             }).FirstOrDefaultAsync();

        if (employee == null)
        {
            throw new Exception("Employee Not Found");
        }

        var employeeForm = new EmployeeForm
        {
            Key = employee.Employee.Key,
            Code = employee.Employee.Code,
            FirstName = employee.Employee.FirstName,
            LastName = employee.Employee.LastName,
            PhotoKey = employee.Employee.AssetKey,
            CompanyKey = employee.Employee.CompanyKey,
            OrganizationKey = employee.Employee.OrganizationKey,
            PositionKey = employee.Employee.PositionKey,
            TitleKey = employee.Employee.TitleKey,
            BranchKey = employee.Employee.BranchKey,
            GradeKey = employee.Employee.GradeKey,
            HireDate = employee.Employee.HireDate,
            Status = employee.Employee.Status,
            DirectSupervisorKey = employee.Employee.DirectSupervisorKey,
            ResignDate = employee.Employee.ResignDate ?? null,
            CorporateEmail = employee.Employee.CorporateEmail ?? String.Empty,
            PhoneExtension = employee.Employee.PhoneExtension ?? String.Empty,
            Company = employee.Company,
            Organization = employee.Organization,
            Position = employee.Position,
            Title = employee.Title,
            Branch = employee.Branch,
            Grade = employee.Grade,
            DirectSupervisor = employee.DirectSupervisor
        };

        if (employee.EmployeePersonal != null)
        {
            employeeForm.EmployeePersonal = new EmployeePersonalForm
            {
                Key = employee.EmployeePersonal.Key,
                EmployeeKey = employee.EmployeePersonal.EmployeeKey,
                NationalityNumber = employee.EmployeePersonal.NationalityNumber,
                NationalityRegistered = employee.EmployeePersonal.NationalityRegistered,
                PlaceOfBirth = employee.EmployeePersonal.PlaceOfBirth,
                DateOfBirth = employee.EmployeePersonal.DateOfBirth,
                Gender = employee.EmployeePersonal.Gender,
                ReligionKey = employee.EmployeePersonal.ReligionKey,
                MaritalStatus = employee.EmployeePersonal.MaritalStatus,
                Address = employee.EmployeePersonal.Address,
                CountryKey = employee.EmployeePersonal.CountryKey,
                ProvinceKey = employee.EmployeePersonal.ProvinceKey,
                CityKey = employee.EmployeePersonal.CityKey,
                PostalCode = employee.EmployeePersonal.PostalCode,
                CurrentAddress = employee.EmployeePersonal.CurrentAddress,
                CurrentCountryKey = employee.EmployeePersonal.CurrentCountryKey,
                CurrentProvinceKey = employee.EmployeePersonal.CurrentProvinceKey,
                CurrentCityKey = employee.EmployeePersonal.CurrentCityKey,
                CurrentPostalCode = employee.EmployeePersonal.CurrentPostalCode,
                PhoneNumber = employee.EmployeePersonal.PhoneNumber,
                Email = employee.EmployeePersonal.Email,
                SocialMedia = employee.EmployeePersonal.SocialMedia,
                EthnicKey = employee.EmployeePersonal.EthnicKey,
                Weight = employee.EmployeePersonal.Weight,
                Height = employee.EmployeePersonal.Height,
                Blood = employee.EmployeePersonal.Blood,
                IsColorBlindness = employee.EmployeePersonal.IsColorBlindness,
                NumOfChild = employee.EmployeePersonal.NumOfChild,
                CitizenNumber = employee.EmployeePersonal.CitizenNumber,
                CitizenRegistered = employee.EmployeePersonal.CitizenRegistered,
                NationalityKey = employee.EmployeePersonal.NationalityKey,
                Employee = employee.Employee,
                Religion = employee.PersonalReligion,
                Country = employee.PersonalCountry,
                Province = employee.PersonalProvince,
                City = employee.PersonalCity,
                CurrentCountry = employee.PersonalCurrentCountry,
                CurrentProvince = employee.PersonalCurrentProvince,
                CurrentCity = employee.PersonalCurrentCity,
                Ethnic = employee.PersonalEthnic,
                Nationality = employee.PersonalNationality
            };
        }

        // Fetch collections separately and materialize the results
        var employeeEducations = await(from employeeEducation in _context.EmployeeEducations
                                       join education in _context.Educations on employeeEducation.EducationKey equals education.Key
                                       where employeeEducation.EmployeeKey == Key && employeeEducation.DeletedAt == null
                                       select new EmployeeEducationForm
                                       {
                                           Key = employeeEducation.Key,
                                           EmployeeKey = employeeEducation.EmployeeKey,
                                           EducationKey = employeeEducation.EducationKey,
                                           GraduatedYear = employeeEducation.GraduatedYear,
                                           Score = employeeEducation.Score,
                                           IsCertificated = employeeEducation.IsCertificated,
                                           EducationName = education.Name
                                       }).ToListAsync();

        employeeEducations = employeeEducations
        .Select((ed, index) =>
        {
            ed.No = index + 1;
            return ed;
        }).ToList();

        var employeeExperiences = await(from employeeExperience in _context.EmployeeExperiences
                                        join position in _context.Positions on employeeExperience.PositionKey equals position.Key
                                        where employeeExperience.EmployeeKey == Key && employeeExperience.DeletedAt == null
                                        select new EmployeeExperienceForm
                                        {
                                            Key = employeeExperience.Key,
                                            EmployeeKey = employeeExperience.EmployeeKey,
                                            CompanyName = employeeExperience.CompanyName,
                                            PositionKey = employeeExperience.PositionKey,
                                            YearStart = employeeExperience.YearStart,
                                            YearEnd = employeeExperience.YearEnd,
                                            PositionName = position.Name
                                        }).ToListAsync();

        employeeExperiences = employeeExperiences
        .Select((ex, index) =>
        {
            ex.No = index + 1;
            return ex;
        }).ToList();

        var employeeFamilies = await _context.EmployeeFamilies.Where(ef => ef.EmployeeKey == Key && ef.DeletedAt == null)
        .Select(ef => new EmployeeFamilyForm
        {
            Key = ef.Key,
            EmployeeKey = ef.EmployeeKey,
            Name = ef.Name,
            Gender = ef.Gender,
            GenderName = ef.Gender.ToString(),
            BoD = ef.BoD,
            Relationship = ef.Relationship,
            RelationshipName = ef.Relationship.ToString(),
            Address = ef.Address,
            PhoneNumber = ef.PhoneNumber
        }).ToListAsync();

        employeeFamilies = employeeFamilies
        .Select((ef, index) => {
            ef.No = index + 1;
            return ef;
        }).ToList();

        var employeeHobbies = await(from employeeHobby in _context.EmployeeHobbies
                                    join hobby in _context.Hobbies on employeeHobby.HobbyKey equals hobby.Key
                                    where employeeHobby.EmployeeKey == Key && employeeHobby.DeletedAt == null
                                    select new EmployeeHobbyForm
                                    {
                                        Key = employeeHobby.Key,
                                        EmployeeKey = employeeHobby.EmployeeKey,
                                        HobbyKey = employeeHobby.HobbyKey,
                                        Level = employeeHobby.Level,
                                        LevelName = employeeHobby.Level.ToString(),
                                        HobbyName = hobby.Name
                                    }).ToListAsync();

        employeeHobbies = employeeHobbies
        .Select((eh, index) =>
        {
            eh.No = index + 1;
            return eh;
        }).ToList();

        var employeeLanguages = await(from employeeLanguage in _context.EmployeeLanguages
                                      join language in _context.Languages on employeeLanguage.LanguageKey equals language.Key
                                      where employeeLanguage.EmployeeKey == Key && employeeLanguage.DeletedAt == null
                                      select new EmployeeLanguageForm
                                      {
                                          Key = employeeLanguage.Key,
                                          EmployeeKey = employeeLanguage.EmployeeKey,
                                          LanguageKey = employeeLanguage.LanguageKey,
                                          SpeakLevel = employeeLanguage.SpeakLevel,
                                          SpeakLevelName = employeeLanguage.SpeakLevel.ToString(),
                                          ListenLevel = employeeLanguage.ListenLevel,
                                          ListenLevelName = employeeLanguage.ListenLevel.ToString(),
                                          LanguageName = language.Name
                                      }).ToListAsync();
        employeeLanguages = employeeLanguages
        .Select((el, index) =>
        {
            el.No = index + 1;
            return el;
        }).ToList();

        var employeeSkills = await(from employeeSkill in _context.EmployeeSkills
                                   join skill in _context.Skills on employeeSkill.SkillKey equals skill.Key
                                   where employeeSkill.EmployeeKey == Key && employeeSkill.DeletedAt == null
                                   select new EmployeeSkillForm
                                   {
                                       Key = employeeSkill.Key,
                                       EmployeeKey = employeeSkill.EmployeeKey,
                                       SkillKey = employeeSkill.SkillKey,
                                       Level = employeeSkill.Level,
                                       LevelName = employeeSkill.Level.ToString(),
                                       IsCertificated = employeeSkill.IsCertificated,
                                       SkillName = skill.Name
                                   }).ToListAsync();

        employeeSkills = employeeSkills
        .Select((es, index) =>
        {
            es.No = index + 1;
            return es;
        }).ToList();

        employeeForm.JsonEmployeeEducations = JsonConvert.SerializeObject(employeeEducations, Formatting.None);
        employeeForm.JsonEmployeeExperiences = JsonConvert.SerializeObject(employeeExperiences, Formatting.None);
        employeeForm.JsonEmployeeFamilies = JsonConvert.SerializeObject(employeeFamilies, Formatting.None);
        employeeForm.JsonEmployeeHobbies = JsonConvert.SerializeObject(employeeHobbies, Formatting.None);
        employeeForm.JsonEmployeeLanguages = JsonConvert.SerializeObject(employeeLanguages, Formatting.None);
        employeeForm.JsonEmployeeSkills = JsonConvert.SerializeObject(employeeSkills, Formatting.None);

        // Add Employee Attendance Information
        var employeeAttendance = await(from ea in _context.EmployeesAttendances
                                       join sh in _context.Shifts on ea.ShiftKey equals sh.Key
                                       join sch in _context.ShiftSchedules on ea.ShiftScheduleKey equals sch.Key
                                        where ea.EmployeeKey == Key
                                        select new EmployeeAttendance
                                        {
                                            Key = ea.Key,
                                            EmployeeKey = ea.EmployeeKey,
                                            FingerPrintID = ea.FingerPrintID,
                                            ShiftKey = ea.ShiftKey,
                                            ShiftScheduleKey = ea.ShiftScheduleKey,
                                            OvertimeMode = ea.OvertimeMode,
                                            Shift = sh,
                                            ShiftSchedule = sch
                                        }).FirstOrDefaultAsync();

        if (employeeAttendance != null)
        {
            var employeeAttendanceDetails = await (from ead in _context.EmployeeAttendanceDetails
                                                   join ea in _context.EmployeesAttendances on ead.EmployeeAttendanceKey equals ea.Key
                                                   where ead.EmployeeAttendanceKey == employeeAttendance.Key && ead.DeletedAt == null
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

            employeeAttendance.EmployeeAttendanceDetails = employeeAttendanceDetails;
        }
        

        var employeeAttendanceForm = employeeAttendance?.ConvertToEmployeeAttendanceForm();
        if (employeeAttendance?.EmployeeAttendanceDetails != null && employeeAttendance.EmployeeAttendanceDetails.Any())
        {
            var employeeAttendanceDetailsForm = employeeAttendance.EmployeeAttendanceDetails
                                                .Select((detail, index) => detail.ConvertToViewModelEmployeeAttendanceDetail(index))
                                                .ToList();
            employeeAttendanceForm.JsonEmployeeAttendanceDetails = JsonConvert.SerializeObject(employeeAttendanceDetailsForm, Formatting.None);
        }
            

        employeeForm.EmployeeAttendance = employeeAttendanceForm;

        // Add Employee Payroll Information
        var employeePayroll = await (from py in _context.EmployeePayrolls
                                     join bk in _context.Banks on py.BankKey equals bk.Key
                                     where py.EmployeeKey == Key
                                     select new EmployeePayroll
                                     {
                                         Key = py.Key,
                                         EmployeeKey = py.EmployeeKey,
                                         TaxNumber = py.TaxNumber,
                                         TaxRegistered = py.TaxRegistered,
                                         TaxAddress = py.TaxAddress,
                                         TaxStatus = py.TaxStatus,
                                         HealthNationalityInsuranceNumber = py.HealthNationalityInsuranceNumber,
                                         HealthNationalityInsuranceRegistered = py.HealthNationalityInsuranceRegistered,
                                         LaborNationalityInsuranceNumber = py.LaborNationalityInsuranceNumber,
                                         LaborNationalityInsuranceRegistered = py.LaborNationalityInsuranceRegistered,
                                         PensionNationalityInsuranceNumber = py.PensionNationalityInsuranceNumber,
                                         PensionNationalityInsuranceRegistered = py.PensionNationalityInsuranceRegistered,
                                         BankKey = py.BankKey,
                                         BankAccountNumber = py.BankAccountNumber,
                                         BankAccountName = py.BankAccountName,
                                         BankAddress = py.BankAddress,
                                         Bank = bk
                                     }).FirstOrDefaultAsync();

        var employeePayrollForm = employeePayroll?.ConvertToEmployeePayrollForm();
        employeeForm.EmployeePayroll = employeePayrollForm;

        return employeeForm;
    }

    public async Task<IEnumerable<DirectSupervisorList>> GetAllDirectSupervisors(Expression<Func<Employee, bool>>[] wheres)
    {
        var queries = _context.Employees.AsQueryable().Where(x => (x.DirectSupervisorKey == Guid.Empty || x.DirectSupervisorKey == null) && x.DeletedAt == null);
        foreach(var where in wheres)
        {
            queries = queries.Where(where);
        }
        var directSupervisors = await queries.Select(x => new DirectSupervisorList
        {
            Key = x.Key,
            Code = x.Code,
            FirstName = x.FirstName,
            LastName = x.LastName ?? String.Empty,
            FullName = (x.FirstName ?? String.Empty) + " " + (x.LastName ?? String.Empty)
        }).ToListAsync();
        return await Task.FromResult(directSupervisors);
    }

    public async Task SaveEmployeeAsync(Employee employee, CancellationToken cancellationToken)
    {
        var existingEmployee = await _context.Employees.FirstOrDefaultAsync(x => x.Key == employee.Key, cancellationToken);
        if (existingEmployee == null)
        {
            employee.Key = Guid.NewGuid();
            await _context.Employees.AddAsync(employee);
        }
        else
        {
            //update existing employee
            employee.CreatedAt = existingEmployee.CreatedAt;
            employee.CreatedBy = existingEmployee.CreatedBy;
            _context.Employees.Entry(existingEmployee).CurrentValues.SetValues(employee);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeePersonalAsync(EmployeePersonal employeePersonal, CancellationToken cancellationToken)
    {
        var existingEmployeePersonal = await _context.EmployeePersonals.FirstOrDefaultAsync(x => x.Key == employeePersonal.Key, cancellationToken);
        if (existingEmployeePersonal == null)
        {
            employeePersonal.Key = Guid.NewGuid();
            await _context.EmployeePersonals.AddAsync(employeePersonal);
        }
        else
        {
            //update existing employee personal
            employeePersonal.CreatedAt = existingEmployeePersonal.CreatedAt;
            employeePersonal.CreatedBy = existingEmployeePersonal.CreatedBy;
            _context.EmployeePersonals.Entry(existingEmployeePersonal).CurrentValues.SetValues(employeePersonal);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeeEducationsAsync(Guid employeeKey, IEnumerable<EmployeeEducation> educations, CancellationToken cancellationToken)
    {
        _ = await _context.EmployeeEducations
                          .Where(e => e.EmployeeKey == employeeKey && e.DeletedAt == null)
                          .ExecuteDeleteAsync();

        await _context.EmployeeEducations.AddRangeAsync(educations, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeeExperiencesAsync(Guid employeeKey, IEnumerable<EmployeeExperience> experiences, CancellationToken cancellationToken)
    {
        _ = await _context.EmployeeExperiences
                          .Where(e => e.EmployeeKey == employeeKey && e.DeletedAt == null)
                          .ExecuteDeleteAsync();

        await _context.EmployeeExperiences.AddRangeAsync(experiences, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeeFamiliesAsync(Guid employeeKey, IEnumerable<EmployeeFamily> families, CancellationToken cancellationToken)
    {
        _ = await _context.EmployeeFamilies
                          .Where(e => e.EmployeeKey == employeeKey && e.DeletedAt == null)
                          .ExecuteDeleteAsync();

        await _context.EmployeeFamilies.AddRangeAsync(families, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeeHobbiesAsync(Guid employeeKey, IEnumerable<EmployeeHobby> hobbies, CancellationToken cancellationToken)
    {
        _ = await _context.EmployeeHobbies
                          .Where(e => e.EmployeeKey == employeeKey && e.DeletedAt == null)
                          .ExecuteDeleteAsync();

        await _context.EmployeeHobbies.AddRangeAsync(hobbies, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeeLanguagesAsync(Guid employeeKey, IEnumerable<EmployeeLanguage> languages, CancellationToken cancellationToken)
    {
        _ = await _context.EmployeeLanguages
                          .Where(e => e.EmployeeKey == employeeKey && e.DeletedAt == null)
                          .ExecuteDeleteAsync();

        await _context.EmployeeLanguages.AddRangeAsync(languages, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeeSkillsAsync(Guid employeeKey, IEnumerable<EmployeeSkill> skills, CancellationToken cancellationToken)
    {
        _ = await _context.EmployeeSkills
                          .Where(e => e.EmployeeKey == employeeKey && e.DeletedAt == null)
                          .ExecuteDeleteAsync();

        await _context.EmployeeSkills.AddRangeAsync(skills, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmployeeForm>> GetCurriculumVitaeReportQuery(CurriculumVitaeReportDto report)
    {
        var emptyEducationList = Enumerable.Empty<EmployeeEducationForm>().ToList();
        var emptyExperienceList = Enumerable.Empty<EmployeeExperienceForm>().ToList();
        var emptyFamilyList = Enumerable.Empty<EmployeeFamilyForm>().ToList();
        var emptyHobbyList = Enumerable.Empty<EmployeeHobbyForm>().ToList();
        var emptyLanguageList = Enumerable.Empty<EmployeeLanguageForm>().ToList();
        var emptySkillList = Enumerable.Empty<EmployeeSkillForm>().ToList();

        var employees = await(from e in _context.Employees
                              join com in _context.Companies on e.CompanyKey equals com.Key
                              join org in _context.Organizations on e.OrganizationKey equals org.Key
                              join pos in _context.Positions on e.PositionKey equals pos.Key
                              join ti in _context.Titles on e.TitleKey equals ti.Key
                              join br in _context.Branches on e.BranchKey equals br.Key
                              join gr in _context.Grades on e.GradeKey equals gr.Key
                              join dr in _context.Employees on e.DirectSupervisorKey equals dr.Key into parentSupervisor
                              from dr in parentSupervisor.DefaultIfEmpty()
                              join p in _context.EmployeePersonals on e.Key equals p.EmployeeKey into employeePersonalGroup
                              from employeePersonal in employeePersonalGroup.DefaultIfEmpty()
                              join rg in _context.Religions on employeePersonal.ReligionKey equals rg.Key into personalReligionGroup
                              from personalReligion in personalReligionGroup.DefaultIfEmpty()
                              join cou in _context.Countries on employeePersonal.CountryKey equals cou.Key into personalCountryGroup
                              from personalCountry in personalCountryGroup.DefaultIfEmpty()
                              join prov in _context.Provinces on employeePersonal.ProvinceKey equals prov.Key into personalProvinceGroup
                              from personalProvince in personalProvinceGroup.DefaultIfEmpty()
                              join cty in _context.Cities on employeePersonal.CityKey equals cty.Key into personalCityGroup
                              from personalCity in personalCityGroup.DefaultIfEmpty()
                              join ccou in _context.Countries on employeePersonal.CurrentCountryKey equals ccou.Key into personalCurrentCountryGroup
                              from personalCurrentCountry in personalCurrentCountryGroup.DefaultIfEmpty()
                              join cprov in _context.Provinces on employeePersonal.CurrentProvinceKey equals cprov.Key into personalCurrentProvinceGroup
                              from personalCurrentProvince in personalCurrentProvinceGroup.DefaultIfEmpty()
                              join ccty in _context.Cities on employeePersonal.CurrentCityKey equals ccty.Key into personalCurrentCityGroup
                              from personalCurrentCity in personalCurrentCityGroup.DefaultIfEmpty()
                              join etc in _context.Ethnics on employeePersonal.EthnicKey equals etc.Key into personalEthnicGroup
                              from personalEthnic in personalEthnicGroup.DefaultIfEmpty()
                              join ast in _context.Assets on e.AssetKey equals ast.Key into assetGroup
                              from asset in assetGroup.DefaultIfEmpty()
                              join py in _context.EmployeePayrolls on e.Key equals py.EmployeeKey into parentPayrollGroup
                              from payroll in parentPayrollGroup.DefaultIfEmpty()
                              join bk in _context.Banks on payroll.BankKey equals bk.Key into payrollBankGroup
                              from payrollBank in payrollBankGroup.DefaultIfEmpty()
                              where (report.EmployeeKey.HasValue ? e.Key == report.EmployeeKey : true) && 
                                    (report.CompanyKey.HasValue ? e.CompanyKey == report.CompanyKey : true) &&
                                    (report.OrganizationKey.HasValue ? e.OrganizationKey == report.OrganizationKey : true) &&
                                    (report.PositionKey.HasValue ? e.PositionKey == report.PositionKey : true) &&
                                    (report.TitleKey.HasValue ? e.TitleKey == report.TitleKey : true) &&
                                    (report.GradeKey.HasValue ? e.GradeKey == report.GradeKey : true) &&
                                    (report.Status.HasValue ? e.Status == report.Status : true) && e.DeletedAt == null
                              select new EmployeeForm
                              {
                                  Key = e.Key,
                                  Code = e.Code,
                                  FirstName = e.FirstName,
                                  LastName = e.LastName,
                                  PhotoKey = e.AssetKey,
                                  CompanyKey = e.CompanyKey,
                                  OrganizationKey = e.OrganizationKey,
                                  PositionKey = e.PositionKey,
                                  TitleKey = e.TitleKey,
                                  BranchKey = e.BranchKey,
                                  GradeKey = e.GradeKey,
                                  HireDate = e.HireDate,
                                  Status = e.Status,
                                  DirectSupervisorKey = e.DirectSupervisorKey,
                                  ResignDate = e.ResignDate ?? null,
                                  CorporateEmail = e.CorporateEmail ?? String.Empty,
                                  PhoneExtension = e.PhoneExtension ?? String.Empty,
                                  Company = com,
                                  Organization = org,
                                  Position = pos,
                                  Title = ti,
                                  Branch = br,
                                  Grade = gr,
                                  DirectSupervisor = dr,
                                  Age = DateTimeDuration.GetAge(employeePersonal),
                                  LongOfJoin = DateTimeDuration.GetLongOfJoin(e),
                                  Asset = asset == null ? null : new AssetForm
                                  {
                                      Key = asset.Key,
                                      FileName = asset.FileName,
                                      OriginalFileName = asset.OriginalFileName,
                                      MimeType = asset.MimeType,
                                      FilePath = $"/uploads/resources/{asset.FileName}" 
                                  },
                                  EmployeePersonal = new EmployeePersonalForm
                                  {
                                      Key = employeePersonal.Key,
                                      EmployeeKey = employeePersonal.EmployeeKey,
                                      NationalityNumber = employeePersonal.NationalityNumber,
                                      NationalityRegistered = employeePersonal.NationalityRegistered,
                                      PlaceOfBirth = employeePersonal.PlaceOfBirth,
                                      DateOfBirth = employeePersonal.DateOfBirth,
                                      Gender = employeePersonal.Gender,
                                      ReligionKey = employeePersonal.ReligionKey,
                                      MaritalStatus = employeePersonal.MaritalStatus,
                                      Address = employeePersonal.Address,
                                      CountryKey = employeePersonal.CountryKey,
                                      ProvinceKey = employeePersonal.ProvinceKey,
                                      CityKey = employeePersonal.CityKey,
                                      PostalCode = employeePersonal.PostalCode,
                                      CurrentAddress = employeePersonal.CurrentAddress,
                                      CurrentCountryKey = employeePersonal.CurrentCountryKey,
                                      CurrentProvinceKey = employeePersonal.CurrentProvinceKey,
                                      CurrentCityKey = employeePersonal.CurrentCityKey,
                                      CurrentPostalCode = employeePersonal.CurrentPostalCode,
                                      PhoneNumber = employeePersonal.PhoneNumber,
                                      Email = employeePersonal.Email,
                                      SocialMedia = employeePersonal.SocialMedia,
                                      EthnicKey = employeePersonal.EthnicKey,
                                      Weight = employeePersonal.Weight,
                                      Height = employeePersonal.Height,
                                      Blood = employeePersonal.Blood,
                                      IsColorBlindness = employeePersonal.IsColorBlindness,
                                      NumOfChild = employeePersonal.NumOfChild,
                                      Employee = employeePersonal.Employee,
                                      Religion = personalReligion,
                                      Country = personalCountry,
                                      Province = personalProvince,
                                      City = personalCity,
                                      CurrentCountry = personalCurrentCountry,
                                      CurrentProvince = personalCurrentProvince,
                                      CurrentCity = personalCurrentCity,
                                      Ethnic = personalEthnic
                                  } ?? null,
                                  EmployeeEducations = (from emd in _context.EmployeeEducations
                                                        join ed in _context.Educations on emd.EducationKey equals ed.Key
                                                        where emd.EmployeeKey == e.Key && emd.DeletedAt == null
                                                        select new EmployeeEducationForm
                                                        {
                                                            Key = emd.Key,
                                                            EmployeeKey = emd.EmployeeKey,
                                                            EducationKey = emd.EducationKey,
                                                            GraduatedYear = emd.GraduatedYear,
                                                            Score = emd.Score,
                                                            IsCertificated = emd.IsCertificated,
                                                            Education = ed,
                                                            Employee = e
                                                        }).ToList() ?? emptyEducationList,
                                  EmployeeExperiences = (from emp in _context.EmployeeExperiences
                                                        join pos in _context.Positions on emp.PositionKey equals pos.Key
                                                        where emp.EmployeeKey == e.Key && emp.DeletedAt == null
                                                        select new EmployeeExperienceForm
                                                        {
                                                            Key = emp.Key,
                                                            EmployeeKey = emp.EmployeeKey,
                                                            CompanyName = emp.CompanyName,
                                                            PositionKey = emp.PositionKey,
                                                            YearStart = emp.YearStart,
                                                            YearEnd = emp.YearEnd,
                                                            Position = pos,
                                                            Employee = e
                                                        }).ToList() ?? emptyExperienceList,
                                  EmployeeFamilies = _context.EmployeeFamilies.Where(ef => ef.EmployeeKey == e.Key && ef.DeletedAt == null)
                                                    .Select(ef => new EmployeeFamilyForm
                                                    {
                                                        Key = ef.Key,
                                                        EmployeeKey = ef.EmployeeKey,
                                                        Name = ef.Name,
                                                        Gender = ef.Gender,
                                                        GenderName = ef.Gender.ToString(),
                                                        BoD = ef.BoD,
                                                        Relationship = ef.Relationship,
                                                        RelationshipName = ef.Relationship.ToString(),
                                                        Address = ef.Address,
                                                        PhoneNumber = ef.PhoneNumber,
                                                        Employee = e
                                                    })
                                                    .ToList() ?? emptyFamilyList,
                                  EmployeeHobbies = (from emh in _context.EmployeeHobbies
                                                    join hb in _context.Hobbies on emh.HobbyKey equals hb.Key
                                                    where emh.EmployeeKey == e.Key && emh.DeletedAt == null
                                                    select new EmployeeHobbyForm
                                                    {
                                                        Key = emh.Key,
                                                        EmployeeKey = emh.EmployeeKey,
                                                        HobbyKey = emh.HobbyKey,
                                                        Level = emh.Level,
                                                        LevelName = emh.Level.ToString(),
                                                        Hobby = hb,
                                                        Employee = e
                                                    }).ToList() ?? emptyHobbyList,
                                  EmployeeLanguages = (from eml in _context.EmployeeLanguages
                                                       join lg in _context.Languages on eml.LanguageKey equals lg.Key
                                                       where eml.EmployeeKey == e.Key && eml.DeletedAt == null
                                                       select new EmployeeLanguageForm
                                                       {
                                                           Key = eml.Key,
                                                           EmployeeKey = eml.EmployeeKey,
                                                           LanguageKey = eml.LanguageKey,
                                                           SpeakLevel = eml.SpeakLevel,
                                                           SpeakLevelName = eml.SpeakLevel.ToString(),
                                                           ListenLevel = eml.ListenLevel,
                                                           ListenLevelName = eml.ListenLevel.ToString(),
                                                           Language = lg,
                                                           Employee = e
                                                       }).ToList() ?? emptyLanguageList,
                                  EmployeeSkills = (from ems in _context.EmployeeSkills
                                                    join sk in _context.Skills on ems.SkillKey equals sk.Key
                                                    where ems.EmployeeKey == e.Key && ems.DeletedAt == null
                                                    select new EmployeeSkillForm
                                                    {
                                                        Key = ems.Key,
                                                        EmployeeKey = ems.EmployeeKey,
                                                        SkillKey = ems.SkillKey,
                                                        Level = ems.Level,
                                                        LevelName = ems.Level.ToString(),
                                                        IsCertificated = ems.IsCertificated,
                                                        Skill = sk,
                                                        Employee = e
                                                    }).ToList() ?? emptySkillList,
                                  EmployeePayroll = new EmployeePayrollForm
                                  {
                                      Key = payroll.Key,
                                      EmployeeKey = payroll.EmployeeKey,
                                      TaxNumber = payroll.TaxNumber,
                                      TaxRegistered = payroll.TaxRegistered,
                                      TaxAddress = payroll.TaxAddress,
                                      TaxStatus = payroll.TaxStatus,
                                      HealthNationalityInsuranceNumber = payroll.HealthNationalityInsuranceNumber,
                                      HealthNationalityInsuranceRegistered = payroll.HealthNationalityInsuranceRegistered,
                                      LaborNationalityInsuranceNumber = payroll.LaborNationalityInsuranceNumber,
                                      LaborNationalityInsuranceRegistered = payroll.LaborNationalityInsuranceRegistered,
                                      PensionNationalityInsuranceNumber = payroll.PensionNationalityInsuranceNumber,
                                      PensionNationalityInsuranceRegistered = payroll.PensionNationalityInsuranceRegistered,
                                      BankKey = payroll.BankKey,
                                      BankAccountNumber = payroll.BankAccountNumber,
                                      BankAccountName = payroll.BankAccountName,
                                      BankAddress = payroll.BankAddress,
                                      Bank = payrollBank
                                  } ?? null
                              }).ToListAsync();

        return employees;
    }

    public async Task LoadEmployeePhotos(IEnumerable<EmployeeForm> employees)
    {
        foreach (var employee in employees)
        {
            if (employee.PhotoKey.HasValue && employee.PhotoKey != Guid.Empty)
            {
                try
                {
                    // Get asset information from database
                    var asset = await _context.Assets
                        .FirstOrDefaultAsync(x => x.Key == employee.PhotoKey);

                    if (asset != null)
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/resources", asset.FileName);

                        if (File.Exists(filePath))
                        {
                            employee.Asset = new AssetForm
                            {
                                Key = asset.Key,
                                FileName = asset.FileName,
                                OriginalFileName = asset.OriginalFileName,
                                MimeType = asset.MimeType,
                                FileData = await File.ReadAllBytesAsync(filePath)
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    employee.Asset = null;
                }
            }
        }
    }

    public async Task<GraphTurnOverData> GetGraphTurnOver(GraphTurnOverDto report)
    {
        if (!report.SelectedYear.HasValue)
            throw new ArgumentException("Selected year is required");

        var startDate = new DateTime(report.SelectedYear.Value, 1, 1);
        var endDate = startDate.AddYears(1).AddDays(-1);

        var query = _context.Employees.AsQueryable().Where(e => e.DeletedAt == null);

        if (report.CompanyKey != Guid.Empty)
            query = query.Where(e => e.CompanyKey == report.CompanyKey);
        if (report.OrganizationKey != Guid.Empty)
            query = query.Where(e => e.OrganizationKey == report.OrganizationKey);
        if (report.PositionKey != Guid.Empty)
            query = query.Where(e => e.PositionKey == report.PositionKey);
        if (report.TitleKey != Guid.Empty)
            query = query.Where(e => e.TitleKey == report.TitleKey);
        if (report.GradeKey != Guid.Empty)
            query = query.Where(e => e.GradeKey == report.GradeKey);
        if (report.Status.HasValue)
            query = query.Where(e => e.Status == report.Status.Value);

        var employees = await query.ToListAsync();

        var monthlyData = new List<MonthlyTurnOverData>();
        for (int month = 1; month <= 12; month ++)
        {
            var currentMonthStart = new DateTime(report.SelectedYear.Value, month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

            var employeesIn = employees.Count(e => 
                          e.HireDate.Year == report.SelectedYear.Value &&
                          e.HireDate.Month == month);

            var employeesOut = employees.Count(e =>
                            e.ResignDate.HasValue &&
                            e.ResignDate.Value.Year == report.SelectedYear.Value &&
                            e.ResignDate.Value.Month == month);

            var employeesExisting = employees.Count(e =>
                                e.HireDate <= currentMonthEnd &&
                                (!e.ResignDate.HasValue || e.ResignDate > currentMonthStart));

            monthlyData.Add(new MonthlyTurnOverData
            {
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                EmployeesIn = employeesIn,
                EmployeesOut = employeesOut,
                EmployeesExisting = employeesExisting
            });
        }

        return new GraphTurnOverData
        {
            Year = report.SelectedYear.Value,
            MonthlyData = monthlyData
        };
    }

    public async Task<PaginatedList<EmployeeForm>> GetEmployeeesWithDetails(GeneralEmployeeReportDto report, PaginationConfig pagination)
    {
        var queries = from e in _context.Employees
                              join com in _context.Companies on e.CompanyKey equals com.Key
                              join org in _context.Organizations on e.OrganizationKey equals org.Key
                              join pos in _context.Positions on e.PositionKey equals pos.Key
                              join ti in _context.Titles on e.TitleKey equals ti.Key
                              join br in _context.Branches on e.BranchKey equals br.Key
                              join gr in _context.Grades on e.GradeKey equals gr.Key
                              join dr in _context.Employees on e.DirectSupervisorKey equals dr.Key into parentSupervisor
                              from dr in parentSupervisor.DefaultIfEmpty()
                              join p in _context.EmployeePersonals on e.Key equals p.EmployeeKey into employeePersonalGroup
                              from employeePersonal in employeePersonalGroup.DefaultIfEmpty()
                              join rg in _context.Religions on employeePersonal.ReligionKey equals rg.Key into personalReligionGroup
                              from personalReligion in personalReligionGroup.DefaultIfEmpty()
                              join cou in _context.Countries on employeePersonal.CountryKey equals cou.Key into personalCountryGroup
                              from personalCountry in personalCountryGroup.DefaultIfEmpty()
                              join prov in _context.Provinces on employeePersonal.ProvinceKey equals prov.Key into personalProvinceGroup
                              from personalProvince in personalProvinceGroup.DefaultIfEmpty()
                              join cty in _context.Cities on employeePersonal.CityKey equals cty.Key into personalCityGroup
                              from personalCity in personalCityGroup.DefaultIfEmpty()
                              join ccou in _context.Countries on employeePersonal.CurrentCountryKey equals ccou.Key into personalCurrentCountryGroup
                              from personalCurrentCountry in personalCurrentCountryGroup.DefaultIfEmpty()
                              join cprov in _context.Provinces on employeePersonal.CurrentProvinceKey equals cprov.Key into personalCurrentProvinceGroup
                              from personalCurrentProvince in personalCurrentProvinceGroup.DefaultIfEmpty()
                              join ccty in _context.Cities on employeePersonal.CurrentCityKey equals ccty.Key into personalCurrentCityGroup
                              from personalCurrentCity in personalCurrentCityGroup.DefaultIfEmpty()
                              join etc in _context.Ethnics on employeePersonal.EthnicKey equals etc.Key into personalEthnicGroup
                              from personalEthnic in personalEthnicGroup.DefaultIfEmpty()
                              join py in _context.EmployeePayrolls on e.Key equals py.EmployeeKey into parentPayrollGroup
                              from payroll in parentPayrollGroup.DefaultIfEmpty()
                              join bk in _context.Banks on payroll.BankKey equals bk.Key into payrollBankGroup
                              from payrollBank in payrollBankGroup.DefaultIfEmpty()
                              where (report.EmployeeKey.HasValue ? e.Key == report.EmployeeKey : true) &&
                                    (report.CompanyKey.HasValue ? e.CompanyKey == report.CompanyKey : true) &&
                                    (report.OrganizationKey.HasValue ? e.OrganizationKey == report.OrganizationKey : true) &&
                                    (report.PositionKey.HasValue ? e.PositionKey == report.PositionKey : true) &&
                                    (report.TitleKey.HasValue ? e.TitleKey == report.TitleKey : true) &&
                                    (report.GradeKey.HasValue ? e.GradeKey == report.GradeKey : true) &&
                                    (report.Status.HasValue ? e.Status == report.Status : true) && e.DeletedAt == null
                              select new
                              {
                                  Employee = e,
                                  EmployeePersonal = employeePersonal,
                                  Company = com,
                                  Organization = org,
                                  Position = pos,
                                  Title = ti,
                                  Branch = br,
                                  Grade = gr,
                                  DirectSupervisor = dr,
                                  PersonalReligion = personalReligion,
                                  PersonalCountry = personalCountry,
                                  PersonalProvince = personalProvince,
                                  PersonalCity = personalCity,
                                  PersonalCurrentCountry = personalCurrentCountry,
                                  PersonalCurrentProvince = personalCurrentProvince,
                                  PersonalCurrentCity = personalCurrentCity,
                                  PersonalEthnic = personalEthnic,
                                  Payroll = payroll,
                                  PayrollBank = payrollBank
                              };

            string search = pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Employee.Code, $"%{search}%") || EF.Functions.ILike(b.EmployeePersonal.NationalityNumber, $"%{search}%") || EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") || EF.Functions.ILike(b.Employee.LastName, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%") || EF.Functions.ILike(b.Position.Name, $"%{search}%"));
            }

            var emptyEducationList = Enumerable.Empty<EmployeeEducationForm>().ToList();
            var emptyExperienceList = Enumerable.Empty<EmployeeExperienceForm>().ToList();
            var emptyFamilyList = Enumerable.Empty<EmployeeFamilyForm>().ToList();
            var emptyHobbyList = Enumerable.Empty<EmployeeHobbyForm>().ToList();
            var emptyLanguageList = Enumerable.Empty<EmployeeLanguageForm>().ToList();
            var emptySkillList = Enumerable.Empty<EmployeeSkillForm>().ToList();

            var employees = await queries.Select(e => new EmployeeForm
            {
                Key = e.Employee.Key,
                Code = e.Employee.Code,
                FirstName = e.Employee.FirstName,
                LastName = e.Employee.LastName,
                PhotoKey = e.Employee.AssetKey,
                CompanyKey = e.Employee.CompanyKey,
                OrganizationKey = e.Employee.OrganizationKey,
                PositionKey = e.Employee.PositionKey,
                TitleKey = e.Employee.TitleKey,
                BranchKey = e.Employee.BranchKey,
                GradeKey = e.Employee.GradeKey,
                HireDate = e.Employee.HireDate,
                Status = e.Employee.Status,
                DirectSupervisorKey = e.Employee.DirectSupervisorKey,
                Company = e.Company,
                Organization = e.Organization,
                Position = e.Position,
                Title = e.Title,
                Branch = e.Branch,
                Grade = e.Grade,
                DirectSupervisor = e.DirectSupervisor,
                ResignDate = e.Employee.ResignDate ?? null,
                CorporateEmail = e.Employee.CorporateEmail ?? String.Empty,
                PhoneExtension = e.Employee.PhoneExtension ?? String.Empty,
                Age = DateTimeDuration.GetAge(e.EmployeePersonal),
                LongOfJoin = DateTimeDuration.GetLongOfJoin(e.Employee),
                EmployeePersonal = new EmployeePersonalForm
                {
                    Key = e.EmployeePersonal.Key,
                    EmployeeKey = e.EmployeePersonal.EmployeeKey,
                    NationalityNumber = e.EmployeePersonal.NationalityNumber,
                    NationalityRegistered = e.EmployeePersonal.NationalityRegistered,
                    PlaceOfBirth = e.EmployeePersonal.PlaceOfBirth,
                    DateOfBirth = e.EmployeePersonal.DateOfBirth,
                    Gender = e.EmployeePersonal.Gender,
                    ReligionKey = e.EmployeePersonal.ReligionKey,
                    MaritalStatus = e.EmployeePersonal.MaritalStatus,
                    Address = e.EmployeePersonal.Address,
                    CountryKey = e.EmployeePersonal.CountryKey,
                    ProvinceKey = e.EmployeePersonal.ProvinceKey,
                    CityKey = e.EmployeePersonal.CityKey,
                    PostalCode = e.EmployeePersonal.PostalCode,
                    CurrentAddress = e.EmployeePersonal.CurrentAddress,
                    CurrentCountryKey = e.EmployeePersonal.CurrentCountryKey,
                    CurrentProvinceKey = e.EmployeePersonal.CurrentProvinceKey,
                    CurrentCityKey = e.EmployeePersonal.CurrentCityKey,
                    CurrentPostalCode = e.EmployeePersonal.CurrentPostalCode,
                    PhoneNumber = e.EmployeePersonal.PhoneNumber,
                    Email = e.EmployeePersonal.Email,
                    SocialMedia = e.EmployeePersonal.SocialMedia,
                    EthnicKey = e.EmployeePersonal.EthnicKey,
                    Weight = e.EmployeePersonal.Weight,
                    Height = e.EmployeePersonal.Height,
                    Blood = e.EmployeePersonal.Blood,
                    IsColorBlindness = e.EmployeePersonal.IsColorBlindness,
                    NumOfChild = e.EmployeePersonal.NumOfChild,
                    Employee = e.EmployeePersonal.Employee,
                    Religion = e.PersonalReligion,
                    Country = e.PersonalCountry,
                    Province = e.PersonalProvince,
                    City = e.PersonalCity,
                    CurrentCountry = e.PersonalCurrentCountry,
                    CurrentProvince = e.PersonalCurrentProvince,
                    CurrentCity = e.PersonalCurrentCity,
                    Ethnic = e.PersonalEthnic
                } ?? null,
                EmployeeEducations = (from emd in _context.EmployeeEducations
                                      join ed in _context.Educations on emd.EducationKey equals ed.Key
                                      where emd.EmployeeKey == e.Employee.Key && emd.DeletedAt == null
                                      select new EmployeeEducationForm
                                      {
                                          Key = emd.Key,
                                          EmployeeKey = emd.EmployeeKey,
                                          EducationKey = emd.EducationKey,
                                          GraduatedYear = emd.GraduatedYear,
                                          Score = emd.Score,
                                          IsCertificated = emd.IsCertificated,
                                          Education = ed
                                      }).ToList() ?? emptyEducationList,
                EmployeeExperiences = (from emp in _context.EmployeeExperiences
                                       join pos in _context.Positions on emp.PositionKey equals pos.Key
                                       where emp.EmployeeKey == e.Employee.Key && emp.DeletedAt == null
                                       select new EmployeeExperienceForm
                                       {
                                           Key = emp.Key,
                                           EmployeeKey = emp.EmployeeKey,
                                           CompanyName = emp.CompanyName,
                                           PositionKey = emp.PositionKey,
                                           YearStart = emp.YearStart,
                                           YearEnd = emp.YearEnd,
                                           Position = pos
                                       }).ToList() ?? emptyExperienceList,
                EmployeeFamilies = _context.EmployeeFamilies.Where(ef => ef.EmployeeKey == e.Employee.Key && ef.DeletedAt == null)
                                                    .Select(ef => new EmployeeFamilyForm
                                                    {
                                                        Key = ef.Key,
                                                        EmployeeKey = ef.EmployeeKey,
                                                        Name = ef.Name,
                                                        Gender = ef.Gender,
                                                        GenderName = ef.Gender.ToString(),
                                                        BoD = ef.BoD,
                                                        Relationship = ef.Relationship,
                                                        RelationshipName = ef.Relationship.ToString(),
                                                        Address = ef.Address,
                                                        PhoneNumber = ef.PhoneNumber
                                                    })
                                                    .ToList() ?? emptyFamilyList,
                EmployeeHobbies = (from emh in _context.EmployeeHobbies
                                   join hb in _context.Hobbies on emh.HobbyKey equals hb.Key
                                   where emh.EmployeeKey == e.Employee.Key && emh.DeletedAt == null
                                   select new EmployeeHobbyForm
                                   {
                                       Key = emh.Key,
                                       EmployeeKey = emh.EmployeeKey,
                                       HobbyKey = emh.HobbyKey,
                                       Level = emh.Level,
                                       LevelName = emh.Level.ToString(),
                                       Hobby = hb,
                                       Employee = e.Employee
                                   }).ToList() ?? emptyHobbyList,
                EmployeeLanguages = (from eml in _context.EmployeeLanguages
                                     join lg in _context.Languages on eml.LanguageKey equals lg.Key
                                     where eml.EmployeeKey == e.Employee.Key && eml.DeletedAt == null
                                     select new EmployeeLanguageForm
                                     {
                                         Key = eml.Key,
                                         EmployeeKey = eml.EmployeeKey,
                                         LanguageKey = eml.LanguageKey,
                                         SpeakLevel = eml.SpeakLevel,
                                         SpeakLevelName = eml.SpeakLevel.ToString(),
                                         ListenLevel = eml.ListenLevel,
                                         ListenLevelName = eml.ListenLevel.ToString(),
                                         Language = lg
                                     }).ToList() ?? emptyLanguageList,
                EmployeeSkills = (from ems in _context.EmployeeSkills
                                  join sk in _context.Skills on ems.SkillKey equals sk.Key
                                  where ems.EmployeeKey == e.Employee.Key && ems.DeletedAt == null
                                  select new EmployeeSkillForm
                                  {
                                      Key = ems.Key,
                                      EmployeeKey = ems.EmployeeKey,
                                      SkillKey = ems.SkillKey,
                                      Level = ems.Level,
                                      LevelName = ems.Level.ToString(),
                                      IsCertificated = ems.IsCertificated,
                                      Skill = sk
                                  }).ToList() ?? emptySkillList,
                EmployeePayroll = new EmployeePayrollForm
                {
                    Key = e.Payroll.Key,
                    EmployeeKey = e.Payroll.EmployeeKey,
                    TaxNumber = e.Payroll.TaxNumber,
                    TaxRegistered = e.Payroll.TaxRegistered,
                    TaxAddress = e.Payroll.TaxAddress,
                    TaxStatus = e.Payroll.TaxStatus,
                    HealthNationalityInsuranceNumber = e.Payroll.HealthNationalityInsuranceNumber,
                    HealthNationalityInsuranceRegistered = e.Payroll.HealthNationalityInsuranceRegistered,
                    LaborNationalityInsuranceNumber = e.Payroll.LaborNationalityInsuranceNumber,
                    LaborNationalityInsuranceRegistered = e.Payroll.LaborNationalityInsuranceRegistered,
                    PensionNationalityInsuranceNumber = e.Payroll.PensionNationalityInsuranceNumber,
                    PensionNationalityInsuranceRegistered = e.Payroll.PensionNationalityInsuranceRegistered,
                    BankKey = e.Payroll.BankKey,
                    BankAccountNumber = e.Payroll.BankAccountNumber,
                    BankAccountName = e.Payroll.BankAccountName,
                    BankAddress = e.Payroll.BankAddress,
                    Bank = e.PayrollBank
                } ?? null
            })
            .PaginatedListAsync(pagination.PageNumber, pagination.PageSize);

        return await Task.FromResult(employees);
    }

    public async Task<Dictionary<string, int>> GetGraphManPower(GraphManPowerDto report)
    {
        // Start with base query joining necessary tables
        var baseQuery = from emp in _context.Employees
                        join empPersonal in _context.EmployeePersonals
                            on emp.Key equals empPersonal.EmployeeKey
                        join empEducation in _context.EmployeeEducations
                            on emp.Key equals empEducation.EmployeeKey into empEduGroup
                        from empEdu in empEduGroup.DefaultIfEmpty()
                        where emp.DeletedAt == null
                        select new
                        {
                            Employee = emp,
                            Personal = empPersonal,
                            Education = empEdu
                        };
        // Apply filters
        if (report.CompanyKey != Guid.Empty)
            baseQuery = baseQuery.Where(x => x.Employee.CompanyKey == report.CompanyKey);

        if (report.OrganizationKey != Guid.Empty)
            baseQuery = baseQuery.Where(x => x.Employee.OrganizationKey == report.OrganizationKey);

        if (report.PositionKey != Guid.Empty)
            baseQuery = baseQuery.Where(x => x.Employee.PositionKey == report.PositionKey);

        if (report.TitleKey != Guid.Empty)
            baseQuery = baseQuery.Where(x => x.Employee.TitleKey == report.TitleKey);

        if (report.GradeKey != Guid.Empty)
            baseQuery = baseQuery.Where(x => x.Employee.GradeKey == report.GradeKey);

        if (report.Status.HasValue)
            baseQuery = baseQuery.Where(x => x.Employee.Status == report.Status.Value);

        var result = new Dictionary<string, int>();

        // Gender Distribution
        if (report.GenderValues.Any())
        {
            var genderQuery = baseQuery
                .Where(x => report.GenderValues.Contains((int)x.Personal.Gender))
                .GroupBy(x => (int)x.Personal.Gender)
                .Select(g => new
                {
                    GenderValue = g.Key,
                    Name = g.Key == (int)Gender.Male ? "Male" : "Female",
                    Count = g.Select(x => x.Employee.Key).Distinct().Count()
                });

            var genderGroups = await genderQuery.ToDictionaryAsync(
                g => $"Gender_{g.Name}",
                g => g.Count
            );

            foreach (var group in genderGroups)
            {
                result.Add(group.Key, group.Value);
            }
        }

        // Education Distribution
        if (report.EducationKeys.Any())
        {
            var educationQuery = baseQuery
                .Join(_context.Educations,
                      emp => emp.Education.EducationKey,
                      edu => edu.Key,
                      (emp, edu) => new { Employee = emp.Employee, Education = edu })
                .Where(x => report.EducationKeys.Contains(x.Education.Key))
                .GroupBy(x => x.Education)
                .Select(g => new
                {
                    Name = g.Key.Name,
                    Count = g.Select(x => x.Employee.Key).Distinct().Count()
                });

            var educationGroups = await educationQuery.ToDictionaryAsync(
                g => $"Education_{g.Name}",
                g => g.Count
            );

            foreach (var group in educationGroups)
            {
                result.Add(group.Key, group.Value);
            }
        }

        // Religion Distribution
        if (report.ReligionKeys.Any())
        {
            var religionQuery = baseQuery
                .Join(_context.Religions,
                      emp => emp.Personal.ReligionKey,
                      rel => rel.Key,
                      (emp, rel) => new { Employee = emp.Employee, Religion = rel })
                .Where(x => report.ReligionKeys.Contains(x.Religion.Key))
                .GroupBy(x => x.Religion)
                .Select(g => new
                {
                    Name = g.Key.Name,
                    Count = g.Select(x => x.Employee.Key).Distinct().Count()
                });

            var religionGroups = await religionQuery.ToDictionaryAsync(
                g => $"Religion_{g.Name}",
                g => g.Count
            );

            foreach (var group in religionGroups)
            {
                result.Add(group.Key, group.Value);
            }
        }

        // Age Distribution
        if (report.AgeRanges.Any())
        {
            // First, get all employees with their ages calculated
            var ageQuery = baseQuery
                .Select(x => new
                {
                    EmployeeKey = x.Employee.Key,
                    Age = (DateTime.Now.Year - x.Personal.DateOfBirth.Year) -
                          (DateTime.Now.DayOfYear < x.Personal.DateOfBirth.DayOfYear ? 1 : 0)
                })
                .Distinct();

            var ageData = await ageQuery.ToListAsync();

            // Then group them by age ranges
            var ageGroups = ageData
                .GroupBy(x => report.AgeRanges
                    .FirstOrDefault(range =>
                    {
                        var bounds = range.Split('-')
                            .Select(int.Parse)
                            .ToArray();
                        return x.Age >= bounds[0] && x.Age <= bounds[1];
                    }))
                .Where(g => g.Key != null)
                .ToDictionary(
                    g => $"Age_{g.Key}",
                    g => g.Count()
                );

            foreach (var group in ageGroups)
            {
                result.Add(group.Key, group.Value);
            }
        }

        return result;
    }

    public async Task SaveEmployeeAttendanceAsync(EmployeeAttendance attendance, CancellationToken cancellationToken)
    {
        var existingAttendance = await _context.EmployeesAttendances.FirstOrDefaultAsync(x => x.Key == attendance.Key, cancellationToken);

        if (existingAttendance == null)
        {
            attendance.Key = Guid.NewGuid();
            await _context.EmployeesAttendances.AddAsync(attendance, cancellationToken);
        }
        else
        {
            //update existing EmployeeAttendance
            attendance.CreatedAt = existingAttendance.CreatedAt;
            attendance.CreatedBy = existingAttendance.CreatedBy;
            _context.EmployeesAttendances.Entry(existingAttendance).CurrentValues.SetValues(attendance);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveEmployeeAttendanceDetailsAsync(Guid attendanceKey, IEnumerable<EmployeeAttendanceDetail> details, CancellationToken cancellationToken)
    {
        _ = await _context.EmployeeAttendanceDetails
                          .Where(x => x.EmployeeAttendanceKey == attendanceKey && x.DeletedAt == null)
                          .ExecuteDeleteAsync();

        await _context.EmployeeAttendanceDetails.AddRangeAsync(details, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveQuota>> GetEmployeeLeaveQuotas(Guid employeeKey)
    {
        var queries = await(from ea in _context.EmployeesAttendances
                                join ead in _context.EmployeeAttendanceDetails on ea.Key equals ead.EmployeeAttendanceKey
                                where ea.EmployeeKey == employeeKey
                                select new EmployeeAttendanceDetail
                                {
                                    Name = ead.Name,
                                    Quota = ead.Quota,
                                    Credit = ead.Credit,
                                    ExpiredAt = ead.ExpiredAt
                                }).ToListAsync();

        var leaveQuotas = queries.Select((detail, index) => new LeaveQuota
        {
            No = index + 1,
            LeaveName = detail.Name,
            Credit = detail.Credit ?? 0,
            Expired = detail.ExpiredAt ?? DateOnly.MinValue
        }).OrderBy(x => x.Expired).ToList();

        return leaveQuotas;
    }

    public async Task<IEnumerable<ApprovalStatusItemList>> GetEmployeeApprovalStatuses(Guid employeeKey)
    {
        var queries = await (from ac in _context.ApprovalConfigs
                             join org in _context.Organizations on ac.OrganizationKey equals org.Key
                             join emp in _context.Employees on org.Key equals emp.OrganizationKey
                             join apr in _context.Approvers on ac.Key equals apr.ApprovalConfigKey
                             where emp.Key == employeeKey && emp.DirectSupervisorKey.HasValue
                             select new Approver
                             {
                                 ApprovalConfigKey = apr.ApprovalConfigKey,
                                 EmployeeKey = apr.EmployeeKey,
                                 Name = apr.Name,
                                 Level = apr.Level,
                                 Action = apr.Action
                             }).ToListAsync();

        var approvalStatuses = queries.Select((detail, index) => new ApprovalStatusItemList
        {
            No = index + 1,
            Approver = detail.Name,
            Action = detail.Action,
            Status = ApprovalStatus.Waiting,
            ApprovalDate = DateOnly.FromDateTime(DateTime.Now),
            ApproverKey = detail.EmployeeKey,
            Level = detail.Level
        });

        return approvalStatuses;
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByHierarchy(Guid? companyKey, Guid? organizationKey, Guid? positionKey, Guid? titleKey)
    {
        // Get All Active Employee
        var query = _context.Employees.AsQueryable().Where(e => e.DeletedAt == null && e.Status != EmployeeStatus.Resign);

        if (companyKey.HasValue)
            query = query.Where(e => e.CompanyKey == companyKey.Value);
        if (organizationKey.HasValue)
            query = query.Where(e => e.OrganizationKey == organizationKey.Value);
        if (positionKey.HasValue)
            query = query.Where(e => e.PositionKey == positionKey.Value);
        if (titleKey.HasValue)
            query = query.Where(e => e.TitleKey == titleKey.Value);

        var employees = await query.ToListAsync();

        return employees;
    }

    public async Task SaveEmployeePayrollAsync(EmployeePayroll employeePayroll, CancellationToken cancellationToken)
    {
        var existingEmployeePayroll = await _context.EmployeePayrolls.FirstOrDefaultAsync(x => x.Key == employeePayroll.Key, cancellationToken);

        if (existingEmployeePayroll == null)
        {
            employeePayroll.Key = Guid.NewGuid();
            await _context.EmployeePayrolls.AddAsync(employeePayroll, cancellationToken);
        }
        else
        {
            //update existing EmployeePayroll
            employeePayroll.CreatedAt = existingEmployeePayroll.CreatedAt;
            employeePayroll.CreatedBy = existingEmployeePayroll.CreatedBy;
            _context.EmployeePayrolls.Entry(existingEmployeePayroll).CurrentValues.SetValues(employeePayroll);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
