using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;
using OnlineVotingWebApp.ViewModels;
using System.Xml;

namespace OnlineVotingWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly OnlineVotingDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, OnlineVotingDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._context = context;
            this._webHostEnvironment = webHostEnvironment;
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
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult AddCandidatePosition()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

                AddToActivityLogs($"Added new candidate position '{model.CandidatePositionName}'");

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
                return RedirectToAction("ViewCandidatePositions");
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
            if (this._context.CandidatePositions.Count() < 1)
            {
                TempData["ErrorMessage"] = "Please add candidate positions first before adding new candidates!";
                return RedirectToAction("ViewCandidates");
            }

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
                    uniqueFileName = ProcessUploadedPhoto(model.Photo);
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
                    LastName = candidate.LastName,
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
                    uniqueFileName = ProcessUploadedPhoto(model.Photo);
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

                    DeletePhoto(tempPhoto, "candidate");
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
                return RedirectToAction("ViewCandidates");
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
                    DeletePhoto(tempPhoto, "candidate");
                }
                AddToActivityLogs($"Deleted candidate '{tempCandidate}'");

                TempData["SuccessMessage"] = "Candidate deleted successfully!";
                return RedirectToAction("ViewCandidates");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ViewEvent()
        {
            try
            {
                VoteEvent? voteEvent = this._context.VoteEvents.FirstOrDefault();
                return View(voteEvent);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (this._context.CandidatePositions.Count() < 1)
            {
                TempData["ErrorMessage"] = "Please add candidate positions first!";
                return RedirectToAction("ViewEvent");
            }

            if (this._context.Candidates.Count() < 1)
            {
                TempData["ErrorMessage"] = "Please add candidates first!";
                return RedirectToAction("ViewEvent");
            }

            if (this._context.VoteEvents.Count() > 0)
            {
                TempData["ErrorMessage"] = "Election event has already been scheduled.";
                return RedirectToAction("ViewEvent");
            }

            var candidates = this._context.Candidates.ToList();

            foreach (var pos in this._context.CandidatePositions)
            {
                int count = candidates.Count(e => e.CandidatePositionId == pos.CandidatePositionId);
                if (count < 2)
                {
                    TempData["ErrorMessage"] = "There must be at least two candidates running for each position to schedule an election.";
                    return RedirectToAction("ViewEvent");
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            int resultStart = DateTime.Compare(DateTime.Now, model.StartDateTime);
            int result = DateTime.Compare(model.StartDateTime, model.EndDateTime);

            if (resultStart > 0)
            {
                TempData["ErrorMessage"] = "The start date time has already passed. Please choose a valid date and time.";
                return View(model);
            }

            if (result > 0)
            {
                TempData["ErrorMessage"] = "The start date time should be before the end date time.";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                VoteEvent voteEvent = new VoteEvent()
                {
                    StartDateTime = model.StartDateTime,
                    EndDateTime = model.EndDateTime
                };

                await this._context.VoteEvents.AddAsync(voteEvent);
                await this._context.SaveChangesAsync();

                AddToActivityLogs($"Added new event");

                TempData["SuccessMessage"] = "Vote event created successfully!";
                return RedirectToAction("ViewEvent");
            }

            TempData["ErrorMessage"] = "Model state not valid!";
            return View(model);
        }

        [HttpGet]
        public IActionResult UpdateEvent(int? id)
        {
            try
            {
                var voteEvent = this._context.VoteEvents.Find(id);

                if (voteEvent == null)
                {
                    return NotFound();
                }

                var model = new UpdateEventViewModel()
                {
                    VoteEventId = voteEvent.VoteEventId,
                    StartDateTime = voteEvent.StartDateTime,
                    EndDateTime = voteEvent.EndDateTime
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
        public async Task<IActionResult> UpdateEvent(UpdateEventViewModel model)
        {
            //int resultStart = DateTime.Compare(DateTime.Now, model.StartDateTime);
            int resultEnd = DateTime.Compare(DateTime.Now, model.EndDateTime);
            int result = DateTime.Compare(model.StartDateTime, model.EndDateTime);

            //if (resultStart > 0)
            //{
            //    TempData["ErrorMessage"] = "The start date time has already passed. Please choose a valid date and time.";
            //    return View(model);
            //}
            if (result >= 0)
            {
                TempData["ErrorMessage"] = "The start date time should be before the end date time.";
                return View(model);
            }

            if (resultEnd >= 0)
            {
                TempData["ErrorMessage"] = "The end date time has already passed. Please choose a valid date and time.";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var voteEvent = this._context.VoteEvents.Find(model.VoteEventId);

                if (voteEvent == null)
                {
                    return NotFound();
                }

                voteEvent.StartDateTime = model.StartDateTime;
                voteEvent.EndDateTime = model.EndDateTime;

                await this._context.SaveChangesAsync();

                AddToActivityLogs("Updated current event");
                TempData["SuccessMessage"] = "Current event successfully updated!";
                return RedirectToAction("ViewEvent");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteEvent(int? id)
        {
            try
            {
                var voteEvent = this._context.VoteEvents.Find(id);

                if (voteEvent == null)
                {
                    return NotFound();
                }

                var model = new UpdateEventViewModel()
                {
                    VoteEventId = voteEvent.VoteEventId,
                    StartDateTime = voteEvent.StartDateTime,
                    EndDateTime = voteEvent.EndDateTime
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("ViewEvent");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(UpdateEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var voteEvent = this._context.VoteEvents.Find(model.VoteEventId);

                if (voteEvent == null)
                {
                    return NotFound();
                }

                this._context.VoteEvents.Remove(voteEvent);
                await this._context.SaveChangesAsync();

                AddToActivityLogs("Deleted event");
                TempData["SuccessMessage"] = "Deleted event successfully!";
                return RedirectToAction("ViewEvent");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ViewResetEvent()
        {
            if (this._context.VoteEvents.Count() < 1)
            {
                TempData["ErrorMessage"] = "There is currently no scheduled event. Schedule an event first.";
                return RedirectToAction("ViewEvent");
            }

            try
            {
                VoteEvent? voteEvent = this._context.VoteEvents.FirstOrDefault();
                return View(voteEvent);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult ResetEvent()
        {
            VoteEvent? voteEvent = this._context.VoteEvents.FirstOrDefault();

            int resultStart = DateTime.Compare(DateTime.Now, voteEvent.StartDateTime);
            int resultEnd = DateTime.Compare(DateTime.Now, voteEvent.EndDateTime);

            if (resultStart < 0)
            {
                TempData["ErrorMessage"] = "The event has not yet started. You cannot reset this event.";
                return RedirectToAction("ViewResetEvent");
            }

            if ((resultStart > 0) && (resultEnd < 0))
            {
                TempData["ErrorMessage"] = "The event is still ongoing. You cannot reset this event.";
                return RedirectToAction("ViewResetEvent");
            }

            TruncateTables();
            AddToActivityLogs("Election reset");

            TempData["SuccessMessage"] = "Election has been successfully reset.";
            return RedirectToAction("ViewEvent");
        }

        [HttpGet]
        public async Task<IActionResult> ManageVoters()
        {
            var voters = await this._userManager.GetUsersInRoleAsync("Voter");
            ViewBag.Votes = await this._context.Votes.ToListAsync();

            return View(voters);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteVoter(string id)
        {
            try
            {
                var voter = await _userManager.FindByIdAsync(id);

                if (voter == null)
                {
                    return NotFound();
                }

                var model = new DeleteVoterViewModel()
                {
                    VoterId = voter.Id,
                    FirstName = voter.FirstName,
                    MiddleName = voter.MiddleName,
                    LastName = voter.LastName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                return RedirectToAction("ManageVoters");
            }
        }

        public async Task<IActionResult> DeleteVoter(DeleteVoterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var voter = await _userManager.FindByIdAsync(model.VoterId);

                if (voter == null)
                {
                    return NotFound();
                }

                var tempName = voter.FullName;
                var tempPhoto = voter.Photo;
                var result = await _userManager.DeleteAsync(voter);

                if (result.Succeeded)
                {
                    if (tempPhoto != null)
                    {
                        DeletePhoto(tempPhoto, "user");
                    }
                    AddToActivityLogs($"Deleted voter '{tempName}'");

                    TempData["SuccessMessage"] = "Voter deleted successfully!";
                    return RedirectToAction("ManageVoters");
                }
                
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

                IdentityResult result = await _roleManager.CreateAsync(identityRole);

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
            var userId = _userManager.GetUserId(User);

            var log = new ActivityLog()
            {
                UserId = userId,
                Description = actionDesc,
            };

            this._context.ActivityLogs.Add(log);
            this._context.SaveChanges();
        }

        private void TruncateTables()
        {
            // Truncate tables
            this._context.Database.ExecuteSqlRaw("DELETE FROM CandidatePositions");
            this._context.Database.ExecuteSqlRaw("DELETE FROM ActivityLogs");
            this._context.Database.ExecuteSqlRaw("DELETE FROM VoteEvent");
            this._context.Database.ExecuteSqlRaw("DELETE FROM Transactions");
            this._context.Database.ExecuteSqlRaw("DELETE FROM Votes");

            // Reseed identity of tables
            this._context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('CandidatePositions', RESEED, 0)");
            this._context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Candidates', RESEED, 0)");
            this._context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('ActivityLogs', RESEED, 0)");
            this._context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('VoteEvent', RESEED, 0)");
            this._context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Transactions', RESEED, 0)");
            this._context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Votes', RESEED, 0)");

            // Delete all candidate photos
            DeleteAllPhotos();
        }

        private string ProcessUploadedPhoto(IFormFile photo)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img/candidate");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photo.CopyTo(fileStream);
            }

            return uniqueFileName;
        }

        public IActionResult DeletePhoto(string fileName, string person)
        {
            try
            {
                string? fullPath = null;

                // Combine the web root path with the file path
                if (person == "candidate")
                {
                    fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/candidate", fileName);
                } 
                else if (person == "user")
                {
                    fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/user", fileName);
                }    

                // Delete the file
                System.IO.File.Delete(fullPath);

                return Ok("File deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting file: {ex.Message}");
            }
        }

        public IActionResult DeleteAllPhotos()
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/candidate");

                System.IO.DirectoryInfo di = new DirectoryInfo(fullPath);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                return Ok("Files deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting files: {ex.Message}.");
            }


        }
    }
}
