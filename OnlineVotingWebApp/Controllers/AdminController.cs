using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;
using OnlineVotingWebApp.ViewModels;

namespace OnlineVotingWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly OnlineVotingDbContext _context;

        public AdminController(RoleManager<IdentityRole> roleManager, OnlineVotingDbContext context)
        {
            this.roleManager = roleManager;
            this._context = context;
        }

        [HttpGet]
        public IActionResult ViewCandidatePositions()
        {
            try
            {
                IEnumerable<CandidatePosition> candidatePositions = this._context.CandidatePositions;
                return View(candidatePositions);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddCandidatePosition()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCandidatePosition(AddCandidatePositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                CandidatePosition candidatePosition = new CandidatePosition()
                {
                    CandidatePositionName = model.CandidatePositionName,
                };

                await this._context.CandidatePositions.AddAsync(candidatePosition);
                await this._context.SaveChangesAsync();

                TempData["success"] = "Candidate Position added successfully!";
                return RedirectToAction("ViewCandidatePositions");
            }

            TempData["ErrorMessage"] = "Model state not valid!";
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "New role added successfully!";
                    return RedirectToAction("Index", "Admin");
                }
            }

            TempData["ErrorMessage"] = "Model state not valid!";
            return View(model);
        }
    }
}
