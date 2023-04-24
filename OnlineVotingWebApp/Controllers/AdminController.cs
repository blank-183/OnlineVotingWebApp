using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, OnlineVotingDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._context = context;
            this.webHostEnvironment = webHostEnvironment;
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
        public IActionResult AddCandidatePosition()
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
        public IActionResult ViewCandidates()
        {
            try
            {
                var candidatesAnon = from candidate in this._context.Candidates
                                     join candidatePosition in this._context.CandidatePositions on candidate.CandidatePositionId
                                     equals candidatePosition.CandidatePositionId
                                     select new
                                     {
                                         CandidateId = candidate.CandidateId,
                                         FullName = candidate.FullName,
                                         CandidatePositionName = candidatePosition.CandidatePositionName,
                                         Party = candidate.Party,
                                         Photo = candidate.Photo
                                     };

                List<ViewCandidatesViewModel> candidates = new List<ViewCandidatesViewModel>();

                foreach (var candidate in candidatesAnon)
                {
                    ViewCandidatesViewModel viewCandidatesViewModel = new ViewCandidatesViewModel()
                    {
                        CandidateId = candidate.CandidateId,
                        FullName = candidate.FullName,
                        CandidatePositionName = candidate.CandidatePositionName,
                        Party = candidate.Party,
                        Photo = candidate.Photo
                    };
                    candidates.Add(viewCandidatesViewModel);
                }

                return View(candidates);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult AddCandidate()
        {
            List<CandidatePosition> candidatePositions = this._context.CandidatePositions.ToList();
            ViewBag.CandidatePositions = candidatePositions;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCandidate(AddCandidateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? uniqueFileName = null;

                if (model.Photo != null)
                {
                    uniqueFileName = ProcessUploadedFile(model.Photo);
                }

                var candidate = new Candidate()
                {
                    CandidatePositionId = model.CandidatePositionId,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Party = model.Party,
                    Photo = uniqueFileName
                };
                await this._context.Candidates.AddAsync(candidate);
                await this._context.SaveChangesAsync();
                AddToActivityLogs($"Added new candidate '{candidate.FullName}'");

                TempData["SuccessMessage"] = "New candidate added successfully!";
                return RedirectToAction("ViewCandidates");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult UpdateCandidate(int? id)
        {
            try
            {
                var candidate = this._context.Candidates.Find(id);

                if (candidate == null)
                {
                    return NotFound();
                }

                var model = new UpdateCandidateViewModel()
                {
                    CandidateId = candidate.CandidateId,
                    FirstName = candidate.FirstName,
                    MiddleName = candidate.MiddleName,
                    LastName= candidate.LastName,
                    Party = candidate.Party,
                };

                List<CandidatePosition> candidatePositions = this._context.CandidatePositions.ToList();
                ViewBag.CandidatePositions = candidatePositions;

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
        public async Task<IActionResult> UpdateCandidate(UpdateCandidateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? uniqueFileName = null;
                string? tempPhoto = null;

                var candidate = this._context.Candidates.Find(model.CandidateId);

                if (candidate == null)
                {
                    return NotFound();
                }

                if (model.Photo != null)
                {
                    uniqueFileName = ProcessUploadedFile(model.Photo);
                    tempPhoto = candidate.Photo;
                }

                if (uniqueFileName != null)
                {
                    candidate.CandidatePositionId = model.CandidatePositionId;
                    candidate.FirstName = model.FirstName;
                    candidate.MiddleName = model.MiddleName;
                    candidate.LastName = model.LastName;
                    candidate.Party = model.Party;
                    candidate.Photo = uniqueFileName;

                    DeletePhoto(tempPhoto);
                }
                else
                {
                    candidate.CandidatePositionId = model.CandidatePositionId;
                    candidate.FirstName = model.FirstName;
                    candidate.MiddleName = model.MiddleName;
                    candidate.LastName = model.LastName;
                    candidate.Party = model.Party;
                }
                await this._context.SaveChangesAsync();
                AddToActivityLogs($"Updated candidate '{candidate.FullName}'");

                TempData["SuccessMessage"] = "Candidate updated successfully!";
                return RedirectToAction("ViewCandidates");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteCandidate(int? id)
        {
            try
            {
                var candidate = this._context.Candidates.Find(id);

                if (candidate == null)
                {
                    return NotFound();
                }

                var model = new UpdateCandidateViewModel()
                {
                    CandidateId = candidate.CandidateId,
                    FirstName = candidate.FirstName,
                    MiddleName = candidate.MiddleName,
                    LastName = candidate.LastName,
                    Party = candidate.Party,
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
        public async Task<IActionResult> DeleteCandidate(UpdateCandidateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var candidate = this._context.Candidates.Find(model.CandidateId);
                string? tempPhoto = null;

                if (candidate == null)
                {
                    return NotFound();
                }
                
                if (candidate.Photo != null)
                {
                    tempPhoto = candidate.Photo;
                }

                var tempCandidate = candidate.FullName;

                this._context.Candidates.Remove(candidate);
                await this._context.SaveChangesAsync();

                if (tempPhoto != null)
                {
                    DeletePhoto(tempPhoto);
                }
                AddToActivityLogs($"Deleted candidate '{tempCandidate}'");

                TempData["SuccessMessage"] = "Candidate deleted successfully!";
                return RedirectToAction("ViewCandidates");
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
        [ValidateAntiForgeryToken]
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
        private string ProcessUploadedFile(IFormFile photo)
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "img/candidate");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photo.CopyTo(fileStream);
            }

            return uniqueFileName;
        }
        public IActionResult DeletePhoto(string fileName)
        {
            try
            {
                // Combine the web root path with the file path
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/candidate", fileName);

                // Delete the file
                System.IO.File.Delete(fullPath);

                // Return success response
                return Ok("File deleted successfully");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during file deletion
                return BadRequest($"Error deleting file: {ex.Message}");
            }
        }
    }
}
