using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saga.Domain.Entities.Employees;
using Saga.WebApi.Infrastructures.Services;
using SidoAgung.WebApi.Common.Model;
using System.Linq.Expressions;
using System.Net;

namespace Saga.WebApi.Controllers;

[Route("wongnormal/[controller]/[action]")]
[ApiController]
public class EmployeeController(IRepositoryService _repositoryService)  : ControllerBase
{
    [HttpGet("{employeekey}")]
    public async Task<ApiResponse<Employee>> GetEmployee(Guid employeekey)
    {
        var employee = await _repositoryService.GetEmployee(employeekey);
        if (employee == null)
        {
            return new ApiResponse<Employee>
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Data Not Found!",
                Data = null
            };
        } 
        return new ApiResponse<Employee>
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Success",
            Data = employee
        };
    }

    [HttpGet("{employeeKey}")]
    public async Task<IActionResult> GetEmployeePersonal(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
        {
            return NotFound("Data tidak ditemukan!");
        }

        var employeePersonal = await _repositoryService.GetEmployeePersonal(employee);
        if (employeePersonal == null)
        {
            return NotFound(new ApiResponse<EmployeePersonal>
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Data tidak ditemukan!",
                Data = null
            });
        }

        return Ok(new ApiResponse<EmployeePersonal>
        {
            StatusCode = HttpStatusCode.OK,
            Message = string.Empty,
            Data = employeePersonal
        });
    }

    [HttpGet("{employeeKey}")]
    public async Task<IActionResult> GetEmployeeEducation(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var attendance = await _repositoryService.GetEmployeeEducation(employee);
        if (attendance == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendance);
    }

    [HttpGet("{employeeKey}")]
    public async Task<IActionResult> GetEmployeeExperience(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var attendance = await _repositoryService.GetEmployeeExperience(employee);
        if (attendance == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendance);
    }

    [HttpGet("{employeeKey}")]
    public async Task<IActionResult> GetEmployeeFamily(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var attendance = await _repositoryService.GetEmployeeFamily(employee);
        if (attendance == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendance);
    }

    [HttpGet("{employeeKey}")]
    public async Task<IActionResult> GetEmployeeHobby(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var attendance = await _repositoryService.GetEmployeeHobby(employee);
        if (attendance == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendance);
    }
    [HttpGet("{employeeKey}")]
    public async Task<IActionResult> GetEmployeeSkill(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var attendance = await _repositoryService.GetEmployeeSkill(employee);
        if (attendance == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendance);
    }

    [HttpGet("{employeeKey}")]

    public async Task<IActionResult> GetEmployeeLanguage(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var attendance = await _repositoryService.GetEmployeeLanguage(employee);
        if (attendance == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendance);
    }
    
    
}
