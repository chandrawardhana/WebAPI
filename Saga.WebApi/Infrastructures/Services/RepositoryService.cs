using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.Persistence.Context;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Saga.WebApi.Infrastructures.Services;

public interface IRepositoryService
{
    /// Employee
    Task<Employee> GetEmployee(Guid key);
    Task<EmployeePersonal> GetEmployeePersonal(Employee employee);
    Task<EmployeeEducation> GetEmployeeEducation(Employee employee); 
    Task<EmployeeExperience> GetEmployeeExperience(Employee employee);
    Task<EmployeeFamily> GetEmployeeFamily(Employee employee);
    Task<EmployeeHobby> GetEmployeeHobby(Employee employee);
    Task<EmployeeSkill> GetEmployeeSkill(Employee employee);
    Task<EmployeeLanguage> GetEmployeeLanguage(Employee employee); 

    /// Employee Payroll
    Task<EmployeePayroll> GetEmployeePayroll(Employee employee);

    /// Employee Attendance
    Task<EmployeeAttendance> GetAttendance(Employee employee);
    Task<EmployeeAttendanceDetail> GetAttendanceDetail(Guid attendantKey);

    

    /// Asset
    Task<FileStreamResult> GetFileStream(Guid assetKey);

    ///Common
    Task<Company> GetCompany(Guid companyKey);
    Task<Organization> GetOrganization(Guid organizationKey);
    Task<Position> GetPosition(Guid positionKey);
    Task<Title> GetTitle(Guid titleKey);
    Task<Grade> GetGrade(Guid gradeKey); 
    Task<Branch> GetBranch(Guid branchKey);
    Task<Bank> GetBank(Guid bankKey);

    Task<Ethnic> GetEthnic(Guid ethnicKey);
    Task<Nationality> GetNationality(Guid nationalityKey);
    Task<Religion> GetReligion(Guid religionKey); 

    Task<Country> GetCountry(Guid countryKey);
    Task<Province> GetProvince(Guid provinceKey);
    Task<City> GetCity(Guid cityKey);

    //Attendance Transaction
    Task<AttendancePointAppList> GetAttendanceTransactionList(Guid employeeKey);
    Task<AttendancePointAppForm> GetAttendanceTransactionDetail(Guid attendantpointkey);
    Task<Result> SaveAttendancePoint(AttendancePointAppDto form);


}
public class RepositoryService(DataContext dataContext) : IRepositoryService
{
    // Employee
    public async Task<Employee> GetEmployee(Guid key)
    {
        var employee = await dataContext.Employees.FindAsync(key);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with key {key} not found.");
        }
        return employee;
    }
    public async Task<EmployeePersonal> GetEmployeePersonal(Employee employee)
    {
        var employeePersonal = await dataContext.EmployeePersonals.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (employeePersonal == null)
        {
            throw new KeyNotFoundException($"Employee with key {employee.Key} not found.");
        }
        return employeePersonal;
    }

    public async Task<EmployeeEducation> GetEmployeeEducation(Employee employee)
    {
        var education = await dataContext.EmployeeEducations.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (education == null)
        {
            throw new KeyNotFoundException($"Education with Employee key {employee.Key} not found.");
        }
        return education;
    }
    public async Task<EmployeeExperience> GetEmployeeExperience(Employee employee)
    {
        var experience = await dataContext.EmployeeExperiences.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (experience == null)
        {
            throw new KeyNotFoundException($"Experience with Employee key {employee.Key} not found.");
        }
        return experience;
    }
    public async Task<EmployeeFamily> GetEmployeeFamily(Employee employee)
    {
        var family = await dataContext.EmployeeFamilies.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with Employee key {employee.Key} not found.");
        }
        return family;
    }
    public async Task<EmployeeHobby> GetEmployeeHobby(Employee employee)
    {
        var hobby = await dataContext.EmployeeHobbies.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (hobby == null)
        {
            throw new KeyNotFoundException($"Hobby with Employee key {employee.Key} not found.");
        }
        return hobby;
    }
    public async Task<EmployeeSkill> GetEmployeeSkill(Employee employee)
    {
        var skill = await dataContext.EmployeeSkills.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (skill == null)
        {
            throw new KeyNotFoundException($"Skill with Employee key {employee.Key} not found.");
        }
        return skill;
    }
    public async Task<EmployeeLanguage> GetEmployeeLanguage(Employee employee)
    {
        var language = await dataContext.EmployeeLanguages.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (language == null)
        {
            throw new KeyNotFoundException($"Language with Employee key {employee.Key} not found.");
        }
        return language;
    }

    /// Employee Payroll
    public async Task<EmployeePayroll> GetEmployeePayroll(Employee employee)
    {
        var payroll = await dataContext.EmployeePayrolls.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (payroll == null)
        {
            throw new KeyNotFoundException($"Payroll with Employee key {employee.Key} not found.");
        }
        return payroll;
    }



    /// Employee Attendance
    public async Task<EmployeeAttendance> GetAttendance(Employee employee)
    {
        var attendance = await dataContext.EmployeesAttendances.FirstOrDefaultAsync(x => x.EmployeeKey == employee.Key);
        if (attendance == null)
        {
            throw new KeyNotFoundException($"Attendance with Employee key {employee.Key} not found.");
        }
        return attendance;
    }
    public async Task<EmployeeAttendanceDetail> GetAttendanceDetail(Guid attendantKey)
    {
        var attendanceDetail = await dataContext.EmployeeAttendanceDetails.FirstOrDefaultAsync(x => x.EmployeeAttendanceKey == attendantKey);
        if (attendanceDetail == null)
        {
            throw new KeyNotFoundException($"Attendance Detail with Employee Attendance key {attendantKey} not found.");
        }
        return attendanceDetail;
    }

    /// Asset File
    private readonly IWebHostEnvironment webHostEnvironment;
    public async Task<FileStreamResult> GetFileStream(Guid assetKey)
    {
        var asset = await dataContext.Assets.FindAsync(assetKey);
        if (asset == null)
        {
            throw new KeyNotFoundException($"Asset with key {assetKey} not found.");
        }

        var filePath = Path.Combine(webHostEnvironment?.ContentRootPath, "Resources/Uploads", asset.FileName);
        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found at {filePath}");
        }
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(fileStream, asset.MimeType);
    }

    ///Common 
    public async Task<Company> GetCompany(Guid companyKey)
    {
        var company = await dataContext.Companies.FindAsync(companyKey);
        if (company == null)
        {
            throw new KeyNotFoundException($"Company with key {companyKey} not found.");
        }
        return company;
    }

    public async Task<Position> GetPosition(Guid positionKey)
    {
        var position = await dataContext.Positions.FindAsync(positionKey);
        if (position == null)
        {
            throw new KeyNotFoundException($"Position with key {positionKey} not found.");
        }
        return position;
    }

    public async Task<Title> GetTitle(Guid titleKey)
    {
        var title = await dataContext.Titles.FindAsync(titleKey);
        if (title == null)
        {
            throw new KeyNotFoundException($"Title with key {titleKey} not found.");
        }
        return title;
    }
    public async Task<Branch> GetBranch(Guid branchKey)
    {
        var branch = await dataContext.Branches.FindAsync(branchKey);
        if (branch == null)
        {
            throw new KeyNotFoundException($"Branch with key {branchKey} not found.");
        }
        return branch;
    }
    public async Task<Grade> GetGrade(Guid gradeKey)
    {
        var grade = await dataContext.Grades.FindAsync(gradeKey);
        if (grade == null)
        {
            throw new KeyNotFoundException($"Grade with key {gradeKey} not found.");
        }
        return grade;
    }
    public async Task<Organization> GetOrganization(Guid organizationCode)
    {
        var organization = await dataContext.Organizations.FirstOrDefaultAsync(x => x.Key == organizationCode);
        if (organization == null)
        {
            throw new KeyNotFoundException($"Organization with code {organizationCode} not found.");
        }
        return organization;
    }

    public async Task<Ethnic> GetEthnic(Guid ethnicKey)
    {
        var ethnic = await dataContext.Ethnics.FindAsync(ethnicKey);
        if (ethnic == null)
        {
            throw new KeyNotFoundException($"Grade with key {ethnicKey} not found.");
        }
        return ethnic;
    }

    public async Task<Nationality> GetNationality(Guid nationalityKey)
    {
        var nasionality = await dataContext.Nationalities.FindAsync(nationalityKey);
        if (nasionality == null)
        {
            throw new KeyNotFoundException($"Nationality with key {nationalityKey} not found.");
        }
        return nasionality;
    }

    public async Task<Country> GetCountry(Guid countryKey)
    {
        var country = await dataContext.Countries.FindAsync(countryKey);
        if (country == null)
        {
            throw new KeyNotFoundException($"Country with key {countryKey} not found.");
        }
        return country;
    }

    public async Task<Province> GetProvince(Guid provinceKey)
    {
        var province = await dataContext.Provinces.FindAsync(provinceKey);
        if (province == null)
        {
            throw new KeyNotFoundException($"Province with key {provinceKey} not found.");
        }
        return province;
    }

    public async Task<City> GetCity(Guid cityKey)
    {
        var city = await dataContext.Cities.FindAsync(cityKey);
        if (city == null)
        {
            throw new KeyNotFoundException($"City with key {cityKey} not found.");
        }
        return city;
    }

    public async Task<Religion> GetReligion(Guid religionKey)
    {
        var religion = await dataContext.Religions.FindAsync(religionKey);
        if (religion == null)
        {
            throw new KeyNotFoundException($"Religion with key {religionKey} not found.");
        }
        return religion;
    }
    public async Task<Bank> GetBank(Guid bankKey)
    {
        var bank = await dataContext.Banks.FindAsync(bankKey);
        if (bank == null)
        {
            throw new KeyNotFoundException($"Bank with key {bankKey} not found.");
        }
        return bank;
    }

    /// Employee Attendance Transaction

    public async Task<AttendancePointAppList> GetAttendanceTransactionList(Guid employeeKey)
    {
        var attendance = await dataContext.AttendancePointApps.FirstOrDefaultAsync(x => x.EmployeeKey == employeeKey);
        if (attendance == null)
        {
            throw new KeyNotFoundException($"Attendance with Employee key {employeeKey} not found.");
        }

        var attendanceListItem = new AttendancePointAppListItem
        {
            Key = attendance.Key,
            EmployeeKey = attendance.EmployeeKey,
            Latitude = attendance.Latitude,
            Longitude = attendance.Longitude,
            InOutMode = attendance.InOutMode,
            AbsenceTime = attendance.AbsenceTime,
        };
        var attendanceListItems = new List<AttendancePointAppListItem> { attendanceListItem };
        var attendanceList = new AttendancePointAppList
        {
            EmployeeSelected = employeeKey,
            Attendances = attendanceListItems
        };
        return attendanceList;
    }
    public async Task<AttendancePointAppForm> GetAttendanceTransactionDetail(Guid attendantpointkey)
    {
        var attendance = await dataContext.AttendancePointApps.FirstOrDefaultAsync(x => x.Key == attendantpointkey);
        if (attendance == null)
        {
            throw new KeyNotFoundException($"Attendance with Employee key {attendantpointkey} not found.");
        }
        var attendanceDetail = new AttendancePointAppForm
        {
            Key = attendance.Key,
            EmployeeKey = attendance.EmployeeKey,
            Latitude = attendance.Latitude,
            Longitude = attendance.Longitude,
            InOutMode = attendance.InOutMode,
            AbsenceTime = attendance.AbsenceTime,
            EmployeeFullName = (attendance.Employee.FirstName ?? String.Empty) + " " + (attendance.Employee.LastName ?? String.Empty)
        };
        return attendanceDetail;
    }



    public async Task<Result> SaveAttendancePoint(AttendancePointAppDto form)
    {

        //    public async Task<Result> Handle()
        //    {
        //        try
        //        {
        //            var attendancePointApp = form.ConvertToEntity();
        //            if (attendancePointApp.Key == Guid.Empty)
        //            {
        //                form.Key = Guid.NewGuid();
        //            }

        //            // Check if Attendance Point App exists
        //            var existingAttendancePointApp = await form.AttendancePointApps.FirstOrDefaultAsync(x => x.Key == attendancePointApp.Key && x.DeletedAt == null);
        //            if (existingAttendancePointApp == null)
        //            {
        //                // Add new Attendance Point App
        //                context.AttendancePointApps.Add(attendancePointApp);
        //            }
        //            else
        //            {
        //                attendancePointApp.CreatedAt = existingAttendancePointApp.CreatedAt;
        //                attendancePointApp.CreatedBy = existingAttendancePointApp.CreatedBy;
        //                context.AttendancePointApps.Entry(existingAttendancePointApp).CurrentValues.SetValues(attendancePointApp);
        //            }

        //            await context.SaveChangesAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            return Result.Failure(new[] { ex.Message });
        //        }

        //        return Result.Success();
        //    }
        //} 
         return Result.Success();
    }
}


