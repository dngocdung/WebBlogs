using System.Text.RegularExpressions;

namespace WebBlogs.Extension
{
    public static class Extension
    {
        public static string ToVnd(this double donGia)
        {
            return donGia.ToString("#,##0") + " VND";
        }
        public static string ToUrlFriendly(this string url)
        {
            var result = url.ToLower().Trim();
            result = Regex.Replace(result, "a", "a");
            result = Regex.Replace(result, "[^a-z0-9-]", "");
            result = Regex.Replace(result, "(-)+", "-");
            return result;

        } 
    }
}
