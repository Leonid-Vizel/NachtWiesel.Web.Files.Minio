using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace NachtWiesel.Web.Files.Minio.Reader;

public interface IMinioFileReaderService
{
    MemoryStream? GetFileStream(string? filePath);
    Task<MemoryStream?> GetFileStreamAsync(string? filePath);
    Task StreamFile(HttpResponse response, string filePath, string? label);
}

public sealed class MinioFileReaderService : IMinioFileReaderService
{
    internal string BucketName { get; set; } = null!;
    internal string? BasePath { get; set; } = null;
    private readonly IMinioClient _minioClient;
    private readonly ILogger _logger;
    private readonly string _name;
    public string Path { get; set; } = "/";
    public MinioFileReaderService(IMinioClient minioClient, ILoggerFactory loggerFactory, string name)
    {
        _name = name;
        _minioClient = minioClient;
        _logger = loggerFactory.CreateLogger($"{nameof(MinioFileReaderService)} [{_name}]");
    }

    private string? GetPath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }
        return $"{BasePath}{filePath}";
    }

    private GetObjectArgs BuildArgs(string completeFilePath, Stream outputStream)
    {
        ArgumentException.ThrowIfNullOrEmpty(completeFilePath, nameof(completeFilePath));
        ArgumentNullException.ThrowIfNull(outputStream);

        return new GetObjectArgs()
            .WithBucket(BucketName)
            .WithObject(completeFilePath)
            .WithCallbackStream((stream) =>
            {
                stream.CopyTo(outputStream);
            });
    }

    public MemoryStream? GetFileStream(string? filePath)
    {
        var completePath = GetPath(filePath);
        if (string.IsNullOrEmpty(completePath))
        {
            return null;
        }
        var outputStream = new MemoryStream();
        var args = BuildArgs(completePath, outputStream);

        try
        {
            Task.Run(async () => await _minioClient.GetObjectAsync(args)).Wait();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading file(bucket: {BucketName}; file: {filePath}): {ex}");
            outputStream.Dispose();
            return null;
        }
        return outputStream;
    }

    public async Task<MemoryStream?> GetFileStreamAsync(string? filePath)
    {
        var completeFilePath = GetPath(filePath);
        if (string.IsNullOrEmpty(completeFilePath))
        {
            return null;
        }
        var outputStream = new MemoryStream();
        var args = BuildArgs(completeFilePath, outputStream);

        try
        {
            await _minioClient.GetObjectAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading file(bucket: {BucketName}; file: {filePath}): {ex}");
            outputStream.Dispose();
            return null;
        }
        return outputStream;
    }

    public async Task StreamFile(HttpResponse response, string filePath, string? label)
    {
        label ??= filePath.Replace("/","_").Replace("\\", "_");

        var completeFilePath = GetPath(filePath);
        if (string.IsNullOrEmpty(completeFilePath))
        {
            return;
        }

        response.ContentType = "application/octet-stream";
        var encodedLabel = label.EncodeUTF8WithSpaces();
        response.Headers.Append("Content-Disposition", $"attachment; filename=\"{encodedLabel}\"");
        var responseStream = response.BodyWriter.AsStream();
        var args = BuildArgs(completeFilePath, responseStream);

        try
        {
            await _minioClient.GetObjectAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading file(bucket: {BucketName}; file: {filePath}): {ex}");
            return;
        }
    }
}