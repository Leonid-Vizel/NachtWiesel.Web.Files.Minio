using NachtWiesel.Web.Files.Minio.Archiver;
using NachtWiesel.Web.Files.Minio.Reader;
using NachtWiesel.Web.Files.Minio.Writer;

namespace NachtWiesel.Web.Files.Minio.Configuration;

public sealed class MinioFileServiceConfiguration
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string? BasePath { get; set; } = null!;
    public string BucketName { get; set; } = null!;
    public bool SSL { get; set; } = true;

    public void Apply(MinioFileWriterService service)
    {
        service.BasePath = BasePath;
        service.BucketName = BucketName;
    }

    public void Apply(MinioFileReaderService service)
    {
        service.BasePath = BasePath;
        service.BucketName = BucketName;
    }

    public void Apply(MinioArchiverService service)
    {
        service.BasePath = BasePath;
        service.BucketName = BucketName;
    }
}