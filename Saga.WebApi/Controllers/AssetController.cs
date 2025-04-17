using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saga.Domain.Entities.Employees;
using Saga.WebApi.Infrastructures.Services;
using SidoAgung.WebApi.Common.Model;
using System.Net;

namespace Saga.WebApi.Controllers;

[Route("wongnormal/[controller]/[action]")]
[ApiController]
public class AssetController(IRepositoryService _repositoryService) : ControllerBase
{
    [HttpGet("{assetkey}")]
    public async Task<ApiResponse<string>> GetFileStream(Guid assetkey)
    {
        var fileStream = await _repositoryService.GetFileStream(assetkey);

        if (fileStream == null)
        {
            return new ApiResponse<string>
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Data Not Found!",
                Data = null
            };
        }
        return new ApiResponse<string>
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Api Response Success",
            Data = fileStream.ToString()
        };
    }
}
