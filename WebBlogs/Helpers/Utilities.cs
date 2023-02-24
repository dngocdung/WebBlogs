using System.Net;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;

namespace WebBlogs.Helpers
{
    public static class Utilities
    {
        
        public static string ToTitleCase(string str)
        {
            string result = str;
            if(!string.IsNullOrEmpty(str))
            {
                var words = str.Split(' ');
                for(int index = 0; index < words.Length; index++)
                {
                    var s = words[index];
                    if(s.Length > 0)
                    {
                        words[index] = s[0].ToString().ToUpper() + s.Substring(1);
                    }
                    
                }
                result = String.Join(" ", words);
                
            }
            return result;
        }
        
        public static int PAGE_SIZE = 5;
        public static string SEOUrl(string url)
        {
            url = url.ToLower();
            url = Regex.Replace(url, @"[áàạảãâấầậẩẫăắằặẳẵ]", "a");
            url = Regex.Replace(url, @"[éèẹẻẽêếềệểễ]", "e");
            url = Regex.Replace(url, @"[óòọỏõôốồộổỗơớờợởỡ]", "o");
            url = Regex.Replace(url, @"[íìịỉĩ]", "i");
            url = Regex.Replace(url, @"[ýỳỵỉỹ]", "y");
            url = Regex.Replace(url, @"[úùụủũưứừựửữ]", "u");
            url = Regex.Replace(url, @"[đ]", "d");

            //2. Chỉ cho phép nhận:[0-9a-z-\s]
            url = Regex.Replace(url.Trim(), @"[^0-9a-z-\s]", "").Trim();
            //xử lý nhiều hơn 1 khoảng trắng --> 1 kt
            url = Regex.Replace(url.Trim(), @"\s+", "-");
            //thay khoảng trắng bằng -
            url = Regex.Replace(url, @"\s", "-");
            while (true)
            {
                if (url.IndexOf("--") != -1)
                {
                    url = url.Replace("--", "-");
                }
                else
                {
                    break;
                }
            }
            return url;
        }
        public static string GetRandomKey(int length = 5)
        {
            string pattern = @"0123456789zxcvbnmasdffghjklqwertyuio[]{}:~!@#$%^&*()+";
            Random rd = new Random();
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]); 
            }
            return sb.ToString();
        }
        public static async Task<string> UploadFile(Microsoft.AspNetCore.Http.IFormFile file, string sDirectory, string newname = null)
        {
            try
            {
                if (newname == null) newname = file.FileName;
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory, newname);
                string path2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory);
                if(!System.IO.Directory.Exists(path2))
                {
                    System.IO.Directory.CreateDirectory(path2);
                }
                var supportedTypes = new[] { "jpg", "jpeg", "png", "gif" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if(!supportedTypes.Contains(fileExt.ToLower()))
                {
                    return null;
                }
                else
                {
                    using(var stream =new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return newname;
                }
                
            }
            catch
            {
                return null;
            }
        }
        
        

    }
}
