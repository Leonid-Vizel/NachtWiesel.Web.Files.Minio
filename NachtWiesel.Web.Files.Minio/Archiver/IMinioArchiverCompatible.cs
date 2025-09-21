namespace NachtWiesel.Web.Files.Minio.Archiver;

public interface IMinioArchiverCompatible
{
    string GetArchivePath();
    string GetArchiveLabel();
}