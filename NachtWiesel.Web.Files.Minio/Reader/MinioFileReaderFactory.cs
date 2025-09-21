using Microsoft.Extensions.Logging;
using Minio;
using NachtWiesel.Web.Files.Minio.Configuration;

namespace NachtWiesel.Web.Files.Minio.Reader;

public interface IMinioFileReaderFactory
{
    IMinioFileReaderService Create(string name = _defaultName);
    private const string _defaultName = "Default";
}

public sealed class MinioFileReaderFactory : IMinioFileReaderFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMinioClient _minioClient;
    private readonly MinioFileServiceConfigurationManager _configManager;
    public MinioFileReaderFactory(IMinioClient minioClient, ILoggerFactory loggerFactory, MinioFileServiceConfigurationManager configManager)
    {
        _minioClient = minioClient;
        _loggerFactory = loggerFactory;
        _configManager = configManager;
    }

    public IMinioFileReaderService Create(string name = _defaultName)
    {
        var service = new MinioFileReaderService(_minioClient, _loggerFactory, name);
        if (name != _defaultName)
        {
            var config = _configManager.Find(name);
            config.Apply(service);
        }
        return service;
    }
    private const string _defaultName = "Default";
}