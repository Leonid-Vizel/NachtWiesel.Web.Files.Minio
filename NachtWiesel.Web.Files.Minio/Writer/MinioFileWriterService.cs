using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace NachtWiesel.Web.Files.Minio.Writer;

public interface IMinioFileWriterService
{
    Task<string?> CreateFileAsync(IFormFile file);
    Task<string?> CreateFileAsync(IBrowserFile file, long maxAllowedSize = 512000L);
    Task<string?> CreateFileAsync(Stream steam, string fileName, string? contentType = null);
    Task<string?> CreateFileAsync(byte[] bytes, string fileName, string? contentType = null);
    void DeleteFile(string? fileName);
    Task DeleteFileAsync(string? filePath);
}

public sealed class MinioFileWriterService : IMinioFileWriterService
{
    internal string BucketName { get; set; } = null!;
    internal string? BasePath { get; set; } = null;
    private readonly IMinioClient _minioClient;
    private readonly ILogger _logger;
    private readonly string _name;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;
    private const string _defaultContentType = "application/octet-stream";
    public MinioFileWriterService(IMinioClient minioClient, ILoggerFactory loggerFactory, string name)
    {
        _name = name;
        _minioClient = minioClient;
        _contentTypeProvider = new();
        _logger = loggerFactory.CreateLogger($"{nameof(MinioFileWriterService)} [{_name}]");
    }

    private string? GetPath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }
        return $"{BasePath}{filePath}";
    }

    public async Task<string?> CreateFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        return await CreateFileAsync(stream, file.FileName, file.ContentType);
    }

    public async Task<string?> CreateFileAsync(IBrowserFile file, long maxAllowedSize = 512000L)
    {
        using var stream = file.OpenReadStream(maxAllowedSize);
        return await CreateFileAsync(stream, file.Name, file.ContentType);
    }

    public async Task<string?> CreateFileAsync(byte[] bytes, string fileName, string? contentType = null)
    {
        using var stream = new MemoryStream(bytes);
        return await CreateFileAsync(stream, fileName, contentType);
    }

    public async Task<string?> CreateFileAsync(Stream stream, string fileName, string? contentType = null)
    {
        string extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(contentType) && !_contentTypeProvider.TryGetContentType(fileName, out contentType))
        {
            contentType = _defaultContentType;
        }

        string filePath;
        string completeFilePath;
        StatObjectArgs statArgs;
        bool fileExists = true;
        do
        {
            var guid = Guid.NewGuid();
            filePath = $"{guid}{extension}";
            completeFilePath = GetPath(filePath)!;
            statArgs = new StatObjectArgs()
                .WithBucket(BucketName)
                .WithObject(completeFilePath);
            try
            {
                var fileStat = await _minioClient.StatObjectAsync(statArgs);
                fileExists = (fileStat == null);
            }
            catch
            {
                fileExists = false;
            }
        }
        while (fileExists);

        var putArgs = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(completeFilePath)
            .WithContentType(contentType)
            .WithObjectSize(stream.Length)
            .WithStreamData(stream);

        try
        {
            await _minioClient.PutObjectAsync(putArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing file(bucket: {BucketName}; file: {filePath}): {ex}");
            return null;
        }

        return filePath;
    }

    private RemoveObjectArgs BuildDeleteArgs(string completeFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(completeFilePath, nameof(completeFilePath));

        return new RemoveObjectArgs()
            .WithBucket(BucketName)
            .WithObject(completeFilePath);
    }

    public void DeleteFile(string? filePath)
    {
        var completeFilePath = GetPath(filePath);
        if (string.IsNullOrEmpty(completeFilePath))
        {
            return;
        }

        var args = BuildDeleteArgs(completeFilePath);

        try
        {
            Task.Run(async () => await _minioClient.RemoveObjectAsync(args)).Wait();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting file(bucket: {BucketName}; file: {filePath}): {ex}");
        }
    }

    public async Task DeleteFileAsync(string? filePath)
    {
        var completeFilePath = GetPath(filePath);
        if (string.IsNullOrEmpty(completeFilePath))
        {
            return;
        }

        var args = BuildDeleteArgs(completeFilePath);

        try
        {
            await _minioClient.RemoveObjectAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting file(bucket: {BucketName}; file: {filePath}): {ex}");
        }
    }
}