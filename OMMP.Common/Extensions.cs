using System.Text;

namespace OMMP.Common;

public static class Extensions
{
    public static string ToBase64(this string source)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
    }

    public static string FromBase64(this string source)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(source));
    }
}