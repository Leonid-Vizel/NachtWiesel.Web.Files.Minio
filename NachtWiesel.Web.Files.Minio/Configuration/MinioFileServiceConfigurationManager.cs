namespace NachtWiesel.Web.Files.Minio.Configuration;

public sealed class MinioFileServiceConfigurationManager
{
    private Dictionary<string, MinioFileServiceConfiguration> ConfigurationDictionary = [];

    public MinioFileServiceConfiguration TryAdd(string name, Action<MinioFileServiceConfiguration> configurationAction)
    {
        var config = new MinioFileServiceConfiguration();
        configurationAction.Invoke(config);
        if (!ConfigurationDictionary.TryAdd(name, config))
        {
            throw new Exception($"Minio file services configuration with name \'{name}\' can't be registered");
        }
        return config;
    }

    public MinioFileServiceConfiguration Find(string name)
    {
        if (!ConfigurationDictionary.TryGetValue(name, out var action))
        {
            throw new Exception($"Minio file services configuration with name \"{name}\" not registered!");
        }
        return action;
    }
}