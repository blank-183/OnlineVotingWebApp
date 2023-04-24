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
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly OnlineVotingDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, OnlineVotingDbContext context)
        {
            this.userManager = userManager;
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
                //string actionDesc = $"Added Candidate Position '{model.CandidatePositionName}'";

                CandidatePosition candidatePosition = new CandidatePosition()
                {
                    CandidatePositionName = model.CandidatePositionName,
                };

                await this._context.CandidatePositions.AddAsync(candidatePosition);

                //var userId = userManager.GetUserId(User);

                //var log = new ActivityLog()
                //{
                //    UserId = userId,
                //    Description = actionDesc,
                //};

                //await this._context.ActivityLogs.AddAsync(log);
                await this._context.SaveChangesAsync();

                AddToActivityLogs($"Added candidate position '{model.CandidatePositionName}'");

                TempData["SuccessMessage"] = "Candidate position added successfully!";
                return RedirectToAction("ViewCandidatePositions");
            }

            TempData["ErrorMessage"] = "Model state not valid!";
            return View(model);
        }

        [HttpGet]
        public IActionResult UpdateCandidatePosition(int? id)
        {
            try
            {
                var candidatePosition = this._context.CandidatePositions.Find(id);

                if (candidatePosition == null)
                {
                    return NotFound();
                }

                var model = new UpdateCandidatePositionViewModel()
                {
                    CandidatePositionId = candidatePosition.CandidatePositionId,
                    CandidatePositionName = candidatePosition.CandidatePositionName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCandidatePosition(UpdateCandidatePositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var candidatePosition = this._context.CandidatePositions.Find(model.CandidatePositionId);

                if (candidatePosition == null)
                {
                    return NotFound();
                }

                var tempPosition = candidatePosition.CandidatePositionName;
                candidatePosition.CandidatePositionName = model.CandidatePositionName;
                
                await this._context.SaveChangesAsync();

                AddToActivityLogs($"Update candidate position '{tempPosition}' to '{model.CandidatePositionName}'");

                TempData["SuccessMessage"] = "Candidate position updated successfully!";
                return RedirectToAction("ViewCandidatePositions");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteCandidatePosition(int? id)
        {
            try
            {
                var candidatePosition = this._context.CandidatePositions.Find(id);

                if (candidatePosition == null)
                {
                    return NotFound();
                }

                var model = new UpdateCandidatePositionViewModel()
                {
                    CandidatePositionId = candidatePosition.CandidatePositionId,
                    CandidatePositionName = candidatePosition.CandidatePositionName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCandidatePosition(UpdateCandidatePositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var candidatePosition = this._context.CandidatePositions.Find(model.CandidatePositionId);

                if (candidatePosition == null)
                {
                    return NotFound();
                }

                var tempPosition = candidatePosition.CandidatePositionName;

                this._context.CandidatePositions.Remove(candidatePosition);
                await this._context.SaveChangesAsync();

                AddToActivityLogs($"Deleted candidate position '{tempPosition}'");

                TempData["SuccessMessage"] = "Candidate position deleted successfully!";
                return RedirectToAction("ViewCandidatePositions");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ViewUserLogs()
        {
            try
            {
                IEnumerable<ActivityLog> activityLogs = this._context.ActivityLogs;

                return View(activityLogs);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
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
                    AddToActivityLogs($"Created new role {model.RoleName}");
                    TempData["SuccessMessage"] = "New role added successfully!";
                    return RedirectToAction("Index", "Admin");
                }
            }

            TempData["ErrorMessage"] = "Model state not valid!";
            return View(model);
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
    }
}
