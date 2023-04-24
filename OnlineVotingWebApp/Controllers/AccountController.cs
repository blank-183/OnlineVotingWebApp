using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Editing;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;
using OnlineVotingWebApp.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace OnlineVotingWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly OnlineVotingDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, OnlineVotingDbContext db,
            IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this._context = db;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (this.User.Identity.IsAuthenticated)
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

                ModelState.AddModelError(string.Empty, "Invalid user login credentials.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (this.User.Identity.IsAuthenticated)
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
                string actionDesc = "User created";

                if (model.Photo != null)
                {
                    uniqueFileName = ProcessUploadedFile(model.Photo);
                }

                var user = new ApplicationUser() 
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
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
                    Province = model.Province,
                    Municipality = model.Municipality,
                    AddressLine = model.AddressLine,
                };

                var log = new ActivityLog()
                {
                    UserId = user.Id,
                    Description = actionDesc
                };

                var result = await userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {

                    await this._context.Addresses.AddAsync(address);
                    await this._context.ActivityLogs.AddAsync(log);
                    await this._context.SaveChangesAsync();
                    await userManager.AddToRoleAsync(user, "Voter");
                
                    TempData["SuccessMessage"] = "Your registration was successful! You may now sign in.";
                    return RedirectToAction("Login", "Account");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            string actionDesc = "Logout";
            var userId = userManager.GetUserId(User);

            var log = new ActivityLog()
            {
                UserId = userId,
                Description = actionDesc,
            };

            await this._context.ActivityLogs.AddAsync(log);
            await this._context.SaveChangesAsync();

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

        private string ProcessUploadedFile(IFormFile photo)
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
