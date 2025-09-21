using System.Text;
using System.Web;

namespace NachtWiesel.Web.Files.Minio;

internal static class SpaceStringEncodingExtension
{
    internal static string EncodeUTF8WithSpaces(this string path)
        => string.Join(' ', path.Split(' ').Select(x => HttpUtility.UrlEncode(x, Encoding.UTF8)));
}