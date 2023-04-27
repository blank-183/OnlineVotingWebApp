using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineVotingWebApp.CustomAttributes;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;

namespace OnlineVotingWebApp.Controllers
{
    [Authorize(Roles = "Voter")]
    public class VoterController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly OnlineVotingDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VoterController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, OnlineVotingDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._context = context;
            this._webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> VotingForm()
        {
            VoteEvent? voteEvent = this._context.VoteEvents.FirstOrDefault();

            if (voteEvent == null)
            {
                TempData["ErrorMessage"] = "No event has been scheduled";
                return RedirectToAction("Index", "Home");
            }

            if (DateTime.Now < voteEvent.StartDateTime)
            {
                TempData["ErrorMessage"] = "Voting is not yet allowed. Please wait for the election event to start.";
                
                return RedirectToAction("Index", "Home");
            }

            if (DateTime.Now > voteEvent.EndDateTime)
            {
                TempData["ErrorMessage"] = "The election has already ended. You are not authorized to access this form.";
                return RedirectToAction("Index", "Home");
            }

            var voterId = this._userManager.GetUserId(User);
            var voter = await this._context.Votes.FirstOrDefaultAsync(e => e.VoterId == voterId);

            if (voter != null)
            {
                return RedirectToAction("AccessDenied", "Error");
            }

            List<CandidatePosition> candidatePositions = await this._context.CandidatePositions.ToListAsync();
            ViewBag.CandidatePositions = candidatePositions;

            List<Candidate> candidates = await this._context.Candidates.ToListAsync();
            ViewBag.Candidates = candidates;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CastVote(List<int> candidateIds)
        {
            VoteEvent? voteEvent = this._context.VoteEvents.FirstOrDefault();

            if (voteEvent == null)
            {
                TempData["ErrorMessage"] = "No event has been scheduled.";
                return RedirectToAction("Index", "Home");
            }

            if (DateTime.Now > voteEvent.EndDateTime)
            {
                TempData["ErrorMessage"] = "You filled out the form after the election has ended. Unfortunately, we were unable to record your votes.";
                return RedirectToAction("Index", "Home");
            }

            var voterId = this._userManager.GetUserId(User);

            if (voterId == null)
            {
                return NotFound();
            }

            var vote = new Vote()
            {
                VoterId = voterId
            };

            await this._context.Votes.AddAsync(vote);
            await this._context.SaveChangesAsync();

            foreach (var candidateId in candidateIds)
            {
                if (candidateId == 0)
                {
                    continue;
                }

                var transaction = new Transaction()
                {
                    VoteId = vote.VoteId,
                    CandidateId = candidateId
                };
                await this._context.Transactions.AddAsync(transaction);
            }

            await this._context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your votes have been recorded. Thank you!";
            AddToActivityLogs("Vote cast");
            
            return RedirectToAction("Index", "Home");
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

    }
}
