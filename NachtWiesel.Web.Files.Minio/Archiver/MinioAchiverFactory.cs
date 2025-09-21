using Microsoft.Extensions.Logging;
using Minio;
using NachtWiesel.Web.Files.Minio.Configuration;
using NachtWiesel.Web.Files.Minio.Reader;

namespace NachtWiesel.Web.Files.Minio.Archiver;

public interface IMinioArchiverFactory
{
    IMinioArchiverService Create(string name = _defaultName);
    private const string _defaultName = "Default";
}

public sealed class MinioAchiverFactory : IMinioArchiverFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMinioFileReaderFactory _fileReaderFactory;
    private readonly MinioFileServiceConfigurationManager _configManager;
    public MinioAchiverFactory(IMinioFileReaderFactory fileReaderFactory, ILoggerFactory loggerFactory, MinioFileServiceConfigurationManager configManager)
    {
        _fileReaderFactory = fileReaderFactory;
        _loggerFactory = loggerFactory;
        _configManager = configManager;
    }

    public IMinioArchiverService Create(string name = _defaultName)
    {
        var fileReader = _fileReaderFactory.Create(name);
        var service = new MinioArchiverService(fileReader, _loggerFactory, name);
        if (name != _defaultName)
        {
            var config = _configManager.Find(name);
            config.Apply(service);
        }
        return service;
    }
    private const string _defaultName = "Default";
}