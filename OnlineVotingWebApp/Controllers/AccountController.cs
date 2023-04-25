using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Editing;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;
using OnlineVotingWebApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace OnlineVotingWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly OnlineVotingDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, OnlineVotingDbContext db,
            IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._context = db;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (this.User.Identity != null && this.User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You are already logged in!";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string actionDesc = "Login";
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(model.Email);

                    var log = new ActivityLog()
                    {
                        UserId = user.Id,
                        Description = actionDesc,
                    };

                    await this._context.ActivityLogs.AddAsync(log);
                    await this._context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid user credentials.");

                var email = await userManager.FindByEmailAsync(model.Email);

                if (email != null)
                {
                    string userId = email.Id;
                    AddToActivityLogs("Failed login attempt", userId);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (this.User.Identity != null && this.User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "You are already logged in!";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? uniqueFileName = null;

                if (model.Photo != null)
                {
                    uniqueFileName = ProcessUploadedPhoto(model.Photo);
                }

                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string firstName = textInfo.ToTitleCase(model.FirstName.ToLower());
                string? middleName = null;
                string lastName = textInfo.ToTitleCase(model.LastName.ToLower());
                string province = textInfo.ToTitleCase(model.Province.ToLower());
                string municipality = textInfo.ToTitleCase(model.Municipality.ToLower());

                if (!string.IsNullOrEmpty(model.MiddleName))
                {
                    middleName = textInfo.ToTitleCase(model.MiddleName.ToLower());
                }

                var user = new ApplicationUser() 
                {
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    Sex = model.Sex,
                    CivilStatus = model.CivilStatus,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    UserName = model.Email, 
                    Email = model.Email,
                    Photo = uniqueFileName
                };

                var address = new Address()
                {
                    UserId = user.Id,
                    Region = model.Region,
                    Province = province,
                    Municipality = municipality,
                    AddressLine = model.AddressLine,
                };

                var result = await userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    await this._context.Addresses.AddAsync(address);
                    await this._context.SaveChangesAsync();
                    await userManager.AddToRoleAsync(user, "Voter");

                    AddToActivityLogs("New voter account created", user.Id);
                
                    TempData["SuccessMessage"] = "Your registration was successful! You may now sign in.";
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            AddToActivityLogs($"Logout");

            TempData["SuccessMessage"] = "User successfully logged out.";
            return RedirectToAction("Login", "Account");
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email is already in use");
            }
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsPhoneNumberInUse(string phoneNumber)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Phone number is already in use");
            }
        }

        private void AddToActivityLogs(string actionDesc)
        {
            var userId = userManager.GetUserId(User);

            var log = new ActivityLog()
            {
                UserId = userId,
                Description = actionDesc,
            };

            this._context.ActivityLogs.Add(log);
            this._context.SaveChanges();
        }

        private void AddToActivityLogs(string actionDesc, string userId)
        {
            var log = new ActivityLog()
            {
                UserId = userId,
                Description = actionDesc,
            };

            this._context.ActivityLogs.Add(log);
            this._context.SaveChanges();
        }

        private string ProcessUploadedPhoto(IFormFile photo)
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "img/user");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photo.CopyTo(fileStream);
            }

            return uniqueFileName;
        }

    }
}
