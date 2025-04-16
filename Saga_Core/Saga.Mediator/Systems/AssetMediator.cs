using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Entities.Systems;
using Saga.Domain.ViewModels.Systems;
using Saga.DomainShared;
using Saga.Persistence.Context;

namespace Saga.Mediator.Systems.AssetMediator;

#region "Get File Stream"
#region "Query"
public sealed record GetFileStreamQuery(Guid AssetKey) : IRequest<Result<FileStreamResult>>;
#endregion
#region "Handler"
public sealed class GetFileStreamQueryHandler : IRequestHandler<GetFileStreamQuery, Result<FileStreamResult>>
{
    private readonly IDataContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public GetFileStreamQueryHandler(IDataContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<Result<FileStreamResult>> Handle(GetFileStreamQuery request, CancellationToken cancellationToken)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.Key == request.AssetKey);
        if (asset == null)
            return Result<FileStreamResult>.Failure(new[] { "File not found." });

        var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources/Uploads", asset.FileName);
        if (!System.IO.File.Exists(filePath))
            return Result<FileStreamResult>.Failure(new[] { "File not found on the server." });

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var fileStreamResult = new FileStreamResult(stream, asset.MimeType)
        {
            FileDownloadName = asset.OriginalFileName
        };

        return Result<FileStreamResult>.Success(fileStreamResult);
    }
}
#endregion
#endregion

#region "Get File Url"
#region "Query"
    public sealed record GetFileUrlQuery(Guid AssetKey) : IRequest<String>;
#endregion
#region "Handler"
    public sealed class GetFileUrlQueryHandler : IRequestHandler<GetFileUrlQuery, String>
    {
        private readonly IDataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GetFileUrlQueryHandler(IDataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<String> Handle(GetFileUrlQuery request, CancellationToken cancellationToken)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(x => x.Key == request.AssetKey);
            if (asset == null)
                throw new InvalidOperationException("File not found.");

            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "/Resources/Uploads/", asset.FileName);
            return filePath;
        }
    }
#endregion
#endregion

#region "Upload File"
#region "Command"
public sealed record UploadFileCommand(IFormFile UploadFile) : IRequest<Result<AssetForm>>;
#endregion
#region "Handler"
    public sealed class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<AssetForm>>
    {
        private readonly IDataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadFileCommandHandler(IDataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<Result<AssetForm>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.UploadFile == null || command.UploadFile.Length == 0)
                    throw new Exception("Upload File is required");

                // Generate unique file name
                var uniqueFileName = $"{Guid.NewGuid()}_{command.UploadFile.FileName}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources/Uploads");

                // Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the logo file to the server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await command.UploadFile.CopyToAsync(fileStream);
                }

                //create asset 
                var asset = new Asset
                {
                    Key = Guid.NewGuid(),
                    FileName = uniqueFileName,
                    OriginalFileName = command.UploadFile.FileName,
                    MimeType = command.UploadFile.ContentType,
                    UploadAt = DateTime.Now
                };

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync(cancellationToken);

                var assetForm = new AssetForm
                {
                    Key = asset.Key,
                    FileName = asset.FileName,
                    OriginalFileName = asset.OriginalFileName,
                    MimeType = asset.MimeType,
                    UploadAt = asset.UploadAt
                };

                return Result<AssetForm>.Success(assetForm);
            }
            catch (Exception ex)
            {
                return Result<AssetForm>.Failure(new[] { ex.Message });
            }
        }
    }
#endregion
#endregion

#region "Delete File"
#region "Command"
public sealed record DeleteFileCommand(Guid AssetKey) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IDataContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DeleteFileCommandHandler(IDataContext context,  IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<Result> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(x => x.Key == command.AssetKey);
            if (asset == null)
                throw new InvalidOperationException("File not found.");

            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources/Uploads", asset.FileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath); // Delete the file from the file system
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync(cancellationToken);
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

#region "Get Multiple File Stream"
#region "Query"
    public sealed record GetMultipleFileStreamQuery(Guid[] AssetKeys) : IRequest<Result<List<FileStreamResult>>>;
#endregion
#region "Handler"
    public sealed class GetMultipleFileStreamQueryHandler : IRequestHandler<GetMultipleFileStreamQuery, Result<List<FileStreamResult>>>
    {
        private readonly IDataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GetMultipleFileStreamQueryHandler(IDataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<Result<List<FileStreamResult>>> Handle(GetMultipleFileStreamQuery request, CancellationToken cancellationToken)
        {
            if (request.AssetKeys == null || !request.AssetKeys.Any())
                return await Task.FromResult(Result<List<FileStreamResult>>.Failure(new[] { "No document keys provided." }));

            //Fetch assets for the given document keys
            var assets = await _context.Assets.Where(x => request.AssetKeys.Contains(x.Key)).ToListAsync(cancellationToken);

            var fileStreams = new List<FileStreamResult>();

            foreach (var asset in assets)
            {
                var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources/Uploads", asset.FileName);

                if (!System.IO.File.Exists(filePath))
                    continue; //Skip file that doesn't exist

                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fileStreamResult = new FileStreamResult(stream, asset.MimeType)
                {
                    FileDownloadName = asset.OriginalFileName
                };

                fileStreams.Add(fileStreamResult);
            }
            if (!fileStreams.Any())
                return await Task.FromResult(Result<List<FileStreamResult>>.Failure(new[] { "No files found for the given document keys." }));
        
            return Result<List<FileStreamResult>>.Success(fileStreams);
        }
    }
#endregion
#endregion



