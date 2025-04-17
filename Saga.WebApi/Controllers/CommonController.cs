using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Entities.Organizations;
using Saga.WebApi.Infrastructures.Services;
using SidoAgung.WebApi.Common.Model;
using System.Net;

namespace Saga.WebApi.Controllers;

[Route("wongnormal/[controller]/[action]")]
[ApiController]
public class CommonController(IRepositoryService _repositoryService) : ControllerBase
{
    [HttpGet("{companykey}")]
    public async Task<ApiResponse<Company>> GetCompany(Guid companykey)
    {
        var company = await _repositoryService.GetCompany(companykey);
        if (company == null)
        {
            return new ApiResponse<Company>
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Data Not Found!",
                Data = null
            };
        }
        return new ApiResponse<Company>
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Success",
            Data = company
        };
    }

    [HttpGet("{organizationkey}")]
    public async Task<ApiResponse<Organization>> GetOrganization(Guid organizationkey)
    {
        var organization = await _repositoryService.GetOrganization(organizationkey);
        if (organization == null)
        {
            return new ApiResponse<Organization>
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Data Not Found!",
                Data = null
            };
        }
        return new ApiResponse<Organization>
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Success",
            Data = organization
        };
    }
    [HttpGet("{positionkey}")]
    public async Task<ApiResponse<Position>> GetPosition(Guid positionkey)
    {
        var position = await _repositoryService.GetPosition(positionkey);
        if (position == null)
        {
            return new ApiResponse<Position>
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Data Not Found!",
                Data = null
            };
        }
        return new ApiResponse<Position>
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Success",
            Data = position
        };
    }

    [HttpGet("{titlekey}")]
    public async Task<IActionResult> GetTitle(Guid titlekey)
    {
        var title = await _repositoryService.GetTitle(titlekey);
        if (title == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(title);
    }
    [HttpGet("{branchkey}")]
    public async Task<IActionResult> GetBranch(Guid branchkey)
    {
        var branch = await _repositoryService.GetBranch(branchkey);
        if (branch == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(branch);
    }
    [HttpGet("{gradekey}")]
    public async Task<IActionResult> GetGrade(Guid gradekey)
    {
        var grade = await _repositoryService.GetGrade(gradekey);
        if (grade == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(grade);
    } 

    [HttpGet("{ethnickey}")]
    public async Task<IActionResult> GetEthnic(Guid ethnickey)
    {
        var ethnic = await _repositoryService.GetEthnic(ethnickey);
        if (ethnic == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(ethnic);
    }

    [HttpGet("{nationalitykey}")]
    public async Task<IActionResult> GetNationality(Guid nationalitykey)
    {
        var nasionality = await _repositoryService.GetNationality(nationalitykey);
        if (nasionality == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(nasionality);
    }

    [HttpGet("{religionkey}")]
    public async Task<IActionResult> GetReligion(Guid religionkey)
    {
        var religion = await _repositoryService.GetReligion(religionkey);
        if (religion == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(religion);
    } 

    [HttpGet("{countryKey}")]
    public async Task<IActionResult> GetCountry(Guid countryKey)
    {
        var country = await _repositoryService.GetCountry(countryKey);
        if (country == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(country);
    }
    [HttpGet("{provinceKey}")]
    public async Task<IActionResult> GetProvince(Guid provinceKey)
    {
        var province = await _repositoryService.GetProvince(provinceKey);
        if (province == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(province);
    }
    [HttpGet("{cityKey}")]
    public async Task<IActionResult> GetCity(Guid cityKey)
    {
        var city = await _repositoryService.GetCity(cityKey);
        if (city == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(city);
    }

    [HttpGet("{bankkey}")]
    public async Task<IActionResult> GetDistrict(Guid bankkey)
    {
        var bank = await _repositoryService.GetBank(bankkey);
        if (bank == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(bank);
    }
}
