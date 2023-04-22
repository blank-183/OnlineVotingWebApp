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
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //string? uniqueFileName = null;

                //if (model.Photo != null)
                //{
                //    uniqueFileName = ProcessUploadedFile(model.Photo);
                //}

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
                    //Photo = uniqueFileName
                };

                var address = new Address()
                {
                    UserId = user.Id,
                    Region = model.Region,
                    Province = model.Province,
                    Municipality = model.Municipality,
                    AddressLine = model.AddressLine,
                };
                var result = await userManager.CreateAsync(user, model.Password);

                await this._context.AddAsync(address);
                await this._context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "New User Created!";

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
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
