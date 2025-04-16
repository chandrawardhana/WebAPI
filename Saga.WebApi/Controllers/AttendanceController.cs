using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.WebApi.Infrastructures.Services;

namespace Saga.WebApi.Controllers;

[Route("wongnormal/[controller]/[action]")]
[ApiController]
public class AttendanceController(IRepositoryService _repositoryService) : ControllerBase
{
     [HttpGet("{employeekey}")]
    public async Task<IActionResult> GetEmployeeAttendance(Guid employeekey)
    {
        var employee = await _repositoryService.GetEmployee(employeekey);
        if (employee == null)
            return NotFound("Data tidak ditemukan!");

        var attendance = await _repositoryService.GetAttendance(employee);
        if (attendance == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendance);
    }
    [HttpGet("{attendantKey}")]
    public async Task<IActionResult> GetEmployeeAttendanceDetail(Guid attendantKey)
    { 
        var attendanceDetail = await _repositoryService.GetAttendanceDetail(attendantKey);
        if (attendanceDetail == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendanceDetail);
    }
    [HttpGet("{employeekey}")]
    public async Task<IActionResult> GetAttendanceTransactionList(Guid employeekey)
    {
        var attendanceList = await _repositoryService.GetAttendanceTransactionList(employeekey);
        if (attendanceList == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendanceList);
    }
    [HttpGet("{attendantpointkey}")]
    public async Task<IActionResult> GetAttendanceTransactionDetail(Guid attendantpointkey)
    {
        var attendanceDetail = await _repositoryService.GetAttendanceTransactionDetail(attendantpointkey);
        if (attendanceDetail == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(attendanceDetail);
    }
    [HttpPost]
    public async Task<IActionResult> AddEmployeeAttendance([FromBody] AttendancePointAppDto attendance)
    {
        if (attendance == null)
            return BadRequest("Data tidak valid!");

        var result = await _repositoryService.SaveAttendancePoint(attendance);
         

        return Ok("Data berhasil ditambahkan!");
    }

}
