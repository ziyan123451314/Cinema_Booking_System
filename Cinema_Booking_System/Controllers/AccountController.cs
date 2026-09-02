using CinemaBookingSystem.Models;
using CinemaBookingSystem.Models.ViewModels;
using CinemaBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRecaptchaService _recaptcha;
        private readonly string _recaptchaSiteKey;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IRecaptchaService recaptcha,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _recaptcha = recaptcha;
            _recaptchaSiteKey = configuration["RecaptchaSettings:SiteKey"] ?? string.Empty;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.RecaptchaSiteKey = _recaptchaSiteKey;
            return View();
        }

        // POST: /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.RecaptchaSiteKey = _recaptchaSiteKey;
            if (!ModelState.IsValid) return View(model);

            // Verify reCAPTCHA
            var captchaToken = Request.Form["g-recaptcha-response"].ToString();
            if (!await _recaptcha.VerifyAsync(captchaToken))
            {
                ModelState.AddModelError(string.Empty, "Please complete the CAPTCHA verification.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt or account is disabled.");
                return View(model);
            }

            // 密码输错开启计数锁定
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "" });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // 锁定提示
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked for 1 minute due to 3 failed login attempts. Please try again later.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.RecaptchaSiteKey = _recaptchaSiteKey;
            return View();
        }

        // POST: /Account/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.RecaptchaSiteKey = _recaptchaSiteKey;
            if (!ModelState.IsValid) return View(model);

            // Verify reCAPTCHA
            var captchaToken = Request.Form["g-recaptcha-response"].ToString();
            if (!await _recaptcha.VerifyAsync(captchaToken))
            {
                ModelState.AddModelError(string.Empty, "Please complete the CAPTCHA verification.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Registration successful! Welcome to CineMax!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied() => View();

        // GET: /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var viewModel = new UserProfileViewModel
            {
                User = user,
                ChangePassword = new ChangePasswordViewModel()
            };

            return View(viewModel);
        }

        // POST: /Account/Profile
        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public async Task<IActionResult> Profile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Address = model.Address;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        // POST: /Account/ChangePassword
        [HttpPost, ValidateAntiForgeryToken, Authorize]
        // 关键修复：加入 [Bind(Prefix = "ChangePassword")]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var viewModel = new UserProfileViewModel
                {
                    User = user,
                    ChangePassword = model
                };
                return View("Profile", viewModel);
            }

            // 验证旧密码并更改为新密码
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password has been reset successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var vm = new UserProfileViewModel
            {
                User = user,
                ChangePassword = model
            };
            return View("Profile", vm);
        }
    }
}