using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebBlogs.Helpers;
using WebBlogs.Models;

namespace WebBlogs.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize()]
    public class PostsController : Controller
    {
        private readonly dbBlogsContext _context;

        public PostsController(dbBlogsContext context)
        {
            _context = context;
        }

        // GET: Admin/Posts
        
        public IActionResult Index(int? page, int catID = 0)
        {
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });

            var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
            if (account == null) return NotFound();

            List<Post> lsPosts = new List<Post>();
            
            if(catID != 0)
            {
                lsPosts = lsPosts = _context.Posts
                    .AsTracking()
                    .Where(x => x.CatId == catID)
                .Include(p => p.Account).Include(p => p.Cat)
                .OrderByDescending(x => x.PostId).ToList();
            }    
            else
            {
                lsPosts = _context.Posts.AsTracking().Include(x => x.Cat)
                    .OrderByDescending(x => x.PostId).ToList();
            }    

            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;

            if(account.RoleId == 3) //Admin
            {
                lsPosts = _context.Posts
                .Include(p => p.Account).Include(p => p.Cat)
                .OrderByDescending(x => x.CatId).ToList();
            }    
            else //Không phải Admin
            {
                lsPosts = _context.Posts
                .Include(p => p.Account).Include(p => p.Cat)
                .Where(x => x.AccountId == account.AccountId)
                .OrderByDescending(x => x.CatId).ToList();
            }    
            
            PagedList<Post> models = new PagedList<Post>(lsPosts.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentCat = catID;
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View(models);
        }

        public IActionResult Filtter(int catID = 0)
        {
            var url = $"/Admin/Posts/Index?catID={catID}";
            if (catID == 0)
            {
                url = $"/Admin/Posts/Index"; 
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        // GET: Admin/Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Admin/Posts/Create
        public IActionResult Create()
        {
            //Kiểm tra quyền truy cập
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });

            //ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId");
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View();
        }

        // POST: Admin/Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreatedDate,Author,AccountId,Tags,CatId,IsHot,IsNewfeed")] Post post, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            //Kiểm tra quyền truy cập
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
            if(account == null) return NotFound();
            if (ModelState.IsValid)
            {
                post.AccountId = account.AccountId;
                post.Author = account.FullName;
                if (post.CatId == null) post.CatId = 1;
                post.CreatedDate = DateTime.Now;
                post.Alias = Utilities.SEOUrl(post.Title);
                //post.Views = 0;
                if(fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string NewName = Utilities.SEOUrl(post.Title) + extension;
                    post.Thumb = await Utilities.UploadFile(fThumb, @"news\", NewName.ToLower());
                }    
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "FullName", post.AccountId);
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // GET: Admin/Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", post.AccountId);
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // POST: Admin/Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreatedDate,Author,AccountId,Tags,CatId,IsHot,IsNewfeed")] Post post, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
            if (account == null) return NotFound();
            //Kiểm tra xem bài viết có đúng là của họ hay không
            if(account.AccountId != 3)
            {
                if (post.AccountId != account.AccountId) return RedirectToAction(nameof(Index));
            }    

            if (ModelState.IsValid)
            {
                post.AccountId = account.AccountId;
                post.Author = account.FullName;
                if (post.CatId == null) post.CatId = 1;
                post.CreatedDate = DateTime.Now;
                post.Alias = Utilities.SEOUrl(post.Title);
                //post.Views = 0;
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string NewName = Utilities.SEOUrl(post.Title) + extension;
                    post.Thumb = await Utilities.UploadFile(fThumb, @"news\", NewName.ToLower());
                }
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", post.AccountId);
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatId", post.CatId);
            return View(post);
        }

        // GET: Admin/Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'dbBlogsContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
