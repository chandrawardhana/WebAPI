using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saga.Domain.Entities.Employees;
using Saga.WebApi.Infrastructures.Services;

namespace Saga.WebApi.Controllers;

[Route("wongnormal/[controller]/[action]")]
[ApiController]
public class PayrollController(IRepositoryService _repositoryService) : ControllerBase
{
    [HttpGet("{employeeKey}")]
    public async Task<IActionResult> GetEmployeePayroll(Guid employeeKey)
    {
        var employee = await _repositoryService.GetEmployee(employeeKey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var payroll = await _repositoryService.GetEmployeePayroll(employee);
        if (payroll == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(payroll);
    }
}
