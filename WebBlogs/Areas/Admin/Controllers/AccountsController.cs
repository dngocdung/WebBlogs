using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebBlogs.Areas.Admin.Models;
using WebBlogs.Extension;
using WebBlogs.Helpers;
using WebBlogs.Models;

namespace WebBlogs.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class AccountsController : Controller
    {
        private readonly dbBlogsContext _context;

        public AccountsController(dbBlogsContext context)
        {
            _context = context;
        }

        // GET: Admin/Accounts
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;
            var lsAccounts = _context.Accounts.Include(a => a.Role)
                .OrderByDescending(x => x.CreatedDate);
            PagedList<Account> models = new PagedList<Account>(lsAccounts, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "Login")]
        public IActionResult Login(string returnUrl = null)
        {
            //Xét tài khoản đã đăng nhập chưa
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID != null) return RedirectToAction("Index", "Home", new { Area = "Admin" });
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "Login")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Account kh = _context.Accounts
                        .Include(p => p.Role)
                        .SingleOrDefault(p => p.Email.ToLower() == model.Email.ToLower().Trim());
                    if (kh == null)
                    {
                        ViewBag.Error = "Thông tin đăng nhập chưa chính xác";
                        return View(model);
                    }
                    //string pass = (model.Password.Trim() + kh.Salt.Trim());//.ToMD5();
                    string pass = model.Password.Trim();
                    if(kh.Password.Trim() != pass)
                    {
                        ViewBag.Error = "Thông tin đăng nhập chưa chính xác";
                        return View(model);
                    }

                    kh.LastLogin = DateTime.Now;
                    _context.Update(kh);
                    await _context.SaveChangesAsync();  

                    var taikhoanID = HttpContext.Session.GetString("AccountId");

                    //Lưu secsion khách hàng
                    HttpContext.Session.SetString("AccountId", kh.AccountId.ToString());
                    var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, kh.FullName),
                        new Claim(ClaimTypes.Email, kh.Email),
                        new Claim("AccountId", kh.AccountId.ToString()),
                        new Claim("RoleId", kh.RoleId.ToString()),
                        new Claim(ClaimTypes.Role, kh.Role.RoleName), //add roleName Admin khi đăng nhập

                    };
                    var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");
                    var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });
                    await HttpContext.SignInAsync(userPrincipal);

                    if(Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }    
                    return RedirectToAction("Index", "Home", new {Area = "Admin"});   
                }
            }
            catch
            {
                return RedirectToAction("Index", "Accounts", new { Area = "Admin" });
            }
            return RedirectToAction("Login", "Accounts", new {Area = "Admin"});
        }

        [Route("dang-xuat.html", Name = "Logout")]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.SignOutAsync();
                HttpContext.Session.Remove("AccountId");
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("doi-mat-khau.html", Name = "ChangePasswprd")]
        [Authorize, HttpGet]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            return View();
        }

        [Route("doi-mat-khau.html", Name = "ChangePasswprd")]
        [Authorize, HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            if(ModelState.IsValid)
            {
                var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
                if (account == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
                try
                {
                    //Kiểm tra mật khẩu đúng hay không
                    string passnow = (model.PasswordNow + account.Salt.Trim());//.ToMD5();
                    if(passnow == account.Password.Trim()) //Đúng mật khẩu
                    {
                        //Tạo mật khẩu mới
                        account.Password = (model.Password + account.Salt.Trim());//.ToMD5();
                        _context.Update(account);
                        _context.SaveChanges();
                        return RedirectToAction("Profile", "Accounts", new { Area = "Admin" });
                    }    
                    else //Mật khẩu không đúng trả lại view
                    {
                        return View();
                    }    
                }
                catch
                {
                    return View();
                }
            }    
            return View();
        }
        [Route("edit-profile.html", Name = "EditProfile")]
        [Authorize, HttpGet]
        public IActionResult EditProfile()
        {
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
            if (account == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            return View(account);
        }

        [Route("edit-profile.html", Name = "EditProfile")]
        [Authorize, HttpGet]
        public IActionResult EditProfile(Account model)
        {
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            
            if(ModelState.IsValid)
            {
                var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
                try
                {
                    account.FullName = model.FullName;
                    account.Phone = model.Phone;
                    account.Email= model.Email;
                    _context.Update(account);
                    _context.SaveChanges();
                    return RedirectToAction("Profile", "Accounts", new { Area = "Admin" });
                }
                catch
                {
                    return View(model);
                }
            }    
            return View();
        }
        // GET: Admin/Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Admin/Accounts/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId");
            return View();
        }

        // POST: Admin/Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,FullName,Email,Phone,Password,Salt,Active,CreatedDate,RoleId,LastLogin")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: Admin/Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // POST: Admin/Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,FullName,Email,Phone,Password,Salt,Active,CreatedDate,RoleId,LastLogin")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: Admin/Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'dbBlogsContext.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
          return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
