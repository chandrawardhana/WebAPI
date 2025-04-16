using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saga.WebApi.Infrastructures.Services;

namespace Saga.WebApi.Controllers;

[Route("wongnormal/[controller]/[action]")]
[ApiController]
public class AssetController(IRepositoryService _repositoryService) : ControllerBase
{
    [HttpGet("{assetkey}")]
    public async Task<IActionResult> GetFileStream(Guid assetkey)
    {
        var fileStream = await _repositoryService.GetFileStream(assetkey);
        if (fileStream == null)
            return NotFound("Data tidak ditemukan!");

        return Ok(fileStream);
    }
}
