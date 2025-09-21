using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minio;
using NachtWiesel.Web.Files.Minio.Archiver;
using NachtWiesel.Web.Files.Minio.Reader;
using NachtWiesel.Web.Files.Minio.Writer;

namespace NachtWiesel.Web.Files.Minio.Configuration;

public static class MinioFileServiceConfigurationManagerDependencyInjectionExtensions
{
    public static IServiceCollection AddMinioFileServiceManager(this IServiceCollection services)
    {
        services.AddMinioFileServiceManagerInternal();
        return services;
    }

    internal static MinioFileServiceConfigurationManager AddMinioFileServiceManagerInternal(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var service = services.FirstOrDefault(x => x.ServiceType == typeof(MinioFileServiceConfigurationManager));
        var manager = service?.ImplementationInstance as MinioFileServiceConfigurationManager;
        if (manager != null)
        {
            return manager;
        }
        manager = new MinioFileServiceConfigurationManager();
        services.TryAddSingleton(manager);
        return manager;
    }

    public static IServiceCollection AddMinioFileServices(this IServiceCollection services, string name, Action<MinioFileServiceConfiguration> configureAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(configureAction);

        var manager = services.AddMinioFileServiceManagerInternal();
        var configuration = manager.TryAdd(name, configureAction);

        services.AddMinio(configureClient => configureClient
            .WithEndpoint(configuration.Endpoint)
            .WithCredentials(configuration.AccessKey, configuration.SecretKey)
            .WithSSL(configuration.SSL)
        .Build());

        services.TryAddSingleton<IMinioArchiverFactory, MinioAchiverFactory>();
        services.TryAddSingleton<IMinioFileWriterFactory, MinioFileWriterFactory>();
        services.TryAddSingleton<IMinioFileReaderFactory, MinioFileReaderFactory>();
        return services;
    }
}