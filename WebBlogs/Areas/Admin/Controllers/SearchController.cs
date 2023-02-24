using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBlogs.Models;

namespace WebBlogs.Areas.Admin.Controllers
{
    
    public class SearchController : Controller
    {
        private readonly dbBlogsContext _context;
        public SearchController(dbBlogsContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult FindBaiViet(string keyword)
        {
            if(keyword != null && keyword.Trim().Length > 3) 
            {
                var ls = _context.Posts.Include(x=>x.Cat).AsNoTracking()
                .Where(x=>x.Title.Contains(keyword)|| x.Contents.Contains(keyword))
                .OrderByDescending(x=>x.CreatedDate).ToList();
                return PartialView("ListBaiVietSearchPartial", ls);
            }
            else
            {
                return PartialView("ListBaiVietSearchPartial", null);
            }    
        }
    }
}
