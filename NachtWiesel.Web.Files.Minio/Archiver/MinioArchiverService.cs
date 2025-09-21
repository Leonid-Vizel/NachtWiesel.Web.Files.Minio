using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NachtWiesel.Web.Files.Minio.Reader;
using System.IO.Compression;

namespace NachtWiesel.Web.Files.Minio.Archiver;

public interface IMinioArchiverService
{
    Task CreateAndStreamZipArchive(HttpResponse response, IEnumerable<IMinioArchiverCompatible> files, string name);
}

public sealed class MinioArchiverService : IMinioArchiverService
{
    internal string BucketName { get; set; } = null!;
    internal string? BasePath { get; set; } = null;
    private readonly IMinioFileReaderService _fileReader;
    private readonly ILogger _logger;
    private readonly string _name;
    public MinioArchiverService(IMinioFileReaderService fileReader, ILoggerFactory loggerFactory, string name)
    {
        _name = name;
        _fileReader = fileReader;
        _logger = loggerFactory.CreateLogger($"{nameof(MinioArchiverService)} [{_name}]");
    }

    public async Task CreateAndStreamZipArchive(HttpResponse response, IEnumerable<IMinioArchiverCompatible> files, string archiveName)
    {
        response.ContentType = "application/octet-stream";
        var encodedName = $"{archiveName}.zip".EncodeUTF8WithSpaces();
        response.Headers.Append("Content-Disposition", $"attachment; filename=\"{encodedName}\"");
        using (var archive = new ZipArchive(response.BodyWriter.AsStream(), ZipArchiveMode.Create))
        {
            foreach (var file in files)
            {
                var path = file.GetArchivePath();
                var extension = Path.GetExtension(path);
                var label = file.GetArchiveLabel();
                using (var fileStream = await _fileReader.GetFileStreamAsync(path))
                {
                    if (fileStream == null)
                    {
                        continue;
                    }

                    var entry = archive.CreateEntry($"{label}{extension}");
                    using (var entryStream = entry.Open())
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }
        }
    }
}