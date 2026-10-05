using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new RegisterViewModel());
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Kiểm tra Email trùng lặp
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            return View(model);
        }

        // Kiểm tra Số điện thoại trùng lặp trong hệ thống Customer
        var existingCustomer = await _context.Customers
            .AnyAsync(c => c.PhoneNumber == model.PhoneNumber.Trim());
        if (existingCustomer)
        {
            ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được sử dụng.");
            return View(model);
        }

        // Tạo Identity User
        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Đảm bảo các Role tồn tại
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Gán Role Customer
            await _userManager.AddToRoleAsync(user, "Customer");

            // Tạo bản ghi Customer tương ứng
            var customer = new Customer
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                UserId = user.Id
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Đăng nhập tự động sau khi đăng ký
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Success"] = "Đăng ký tài khoản thành công!";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        IdentityUser? user = null;

        // Thử tìm kiếm theo Email
        if (model.EmailOrPhone.Contains("@"))
        {
            user = await _userManager.FindByEmailAsync(model.EmailOrPhone);
        }
        else
        {
            // Thử tìm theo Số điện thoại trong bảng Customers
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == model.EmailOrPhone.Trim());
            if (customer != null && !string.IsNullOrEmpty(customer.UserId))
            {
                user = await _userManager.FindByIdAsync(customer.UserId);
            }
        }

        if (user == null)
        {
            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // Đăng nhập bằng SignInManager
        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            TempData["Success"] = "Đăng nhập thành công!";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác.");
        return View(model);
    }

    // GET: /admin/login
    [HttpGet("/admin/login")]
    public IActionResult AdminLogin(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                return RedirectToAction("Index", "Admin");
            }
            _signInManager.SignOutAsync().Wait();
        }
        ViewData["ReturnUrl"] = returnUrl ?? "/admin";
        return View(new LoginViewModel());
    }

    // POST: /admin/login
    [HttpPost("/admin/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ?? "/admin";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        IdentityUser? user = null;
        if (model.EmailOrPhone.Contains("@"))
        {
            user = await _userManager.FindByEmailAsync(model.EmailOrPhone);
        }
        else
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == model.EmailOrPhone.Trim());
            if (customer != null && !string.IsNullOrEmpty(customer.UserId))
            {
                user = await _userManager.FindByIdAsync(customer.UserId);
            }
        }

        if (user == null)
        {
            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác.");
            return View(model);
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("Staff"))
        {
            ModelState.AddModelError("", "Tài khoản này không có quyền truy cập trang quản trị.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            TempData["Success"] = "Đăng nhập trang quản lý thành công!";
            return Redirect(returnUrl ?? "/admin");
        }

        ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác.");
        return View(model);
    }


    // GET: /Account/Logout
    [HttpGet("/Account/Logout")]
    public async Task<IActionResult> LogoutGet()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Đã đăng xuất tài khoản thành công.";
        return RedirectToAction("Index", "Home");
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Đã đăng xuất tài khoản.";
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Profile
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = _userManager.GetUserId(User);
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
        {
            // Nếu là admin không có Customer record
            var user = await _userManager.FindByIdAsync(userId!);
            return View(new ProfileViewModel
            {
                FullName = "Administrator",
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                Address = ""
            });
        }

        var model = new ProfileViewModel
        {
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = User.Identity?.Name ?? "",
            Address = customer.Address
        };

        return View(model);
    }

    // POST: /Account/Profile
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer != null)
        {
            // Kiểm tra trùng SĐT với người khác
            var phoneExists = await _context.Customers
                .AnyAsync(c => c.PhoneNumber == model.PhoneNumber.Trim() && c.UserId != userId);
            if (phoneExists)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được sử dụng bởi tài khoản khác.");
                return View(model);
            }

            customer.FullName = model.FullName.Trim();
            customer.PhoneNumber = model.PhoneNumber.Trim();
            customer.Address = model.Address?.Trim();

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            // Cập nhật SĐT trong IdentityUser
            var user = await _userManager.FindByIdAsync(userId!);
            if (user != null)
            {
                user.PhoneNumber = model.PhoneNumber.Trim();
                await _userManager.UpdateAsync(user);
            }

            TempData["Success"] = "Cập nhật hồ sơ cá nhân thành công!";
        }
        else
        {
            // Đang đăng nhập bằng tài khoản không có bản ghi Customer (như admin)
            var user = await _userManager.FindByIdAsync(userId!);
            if (user != null)
            {
                user.PhoneNumber = model.PhoneNumber.Trim();
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Cập nhật thông tin admin thành công!";
            }
        }

        return RedirectToAction(nameof(Profile));
    }

    // POST: /Account/ChangePassword
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PasswordError"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Profile));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Đổi mật khẩu thành công!";
        }
        else
        {
            TempData["PasswordError"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Profile));
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
