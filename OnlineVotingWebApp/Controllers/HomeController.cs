using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.CodeAnalysis.Options;
using OnlineVotingWebApp.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace OnlineVotingWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OnlineVotingDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, OnlineVotingDbContext context)
        {
            _logger = logger;
            this._userManager = userManager;
            this._context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VoteResults()
        {
            VoteEvent? voteEvent = await this._context.VoteEvents.FirstOrDefaultAsync();

            if (voteEvent == null)
            {
                TempData["ErrorMessage"] = "No event has been scheduled";
                return RedirectToAction("Index", "Home");
            }

            // Get all positions
            var candidatePositions = await this._context.CandidatePositions.ToListAsync();

            // Get all candidates
            var candidates = this._context.Candidates.ToList();

            var transactions = from transaction in this._context.Transactions
                                   join candidate in this._context.Candidates on transaction.CandidateId
                                   equals candidate.CandidateId
                                   select new
                                   {
                                       TransactionId = transaction.TransactionId,
                                       VoteId = transaction.VoteId,
                                       CandidateId = candidate.CandidateId,
                                       CandidatePositionId = candidate.CandidatePositionId,
                                   };

            // Get total number of votes in each position
            Dictionary<int, int> positionVoteCount = new Dictionary<int, int>();
            foreach (var pos in candidatePositions)
            {
                int totalVotes = transactions.Count(x => x.CandidatePositionId == pos.CandidatePositionId);
                positionVoteCount.Add(pos.CandidatePositionId, totalVotes);
            }

            // Get total number of votes for each candidate
            Dictionary<int, int> candidateVoteCount = new Dictionary<int, int>();
            foreach (var candidate in candidates)
            {
                int voteCount = transactions.Count(c => c.CandidateId == candidate.CandidateId);
                candidateVoteCount.Add(candidate.CandidateId, voteCount);
            }

            // Get total number of registered voters who voted
            var votes = this._context.Votes.ToList().Count;

            // Get total number of registered voters
            var voters = await this._userManager.GetUsersInRoleAsync("Voter");
            int totalVoters = voters.Count();

            VoteResultViewModel result = new VoteResultViewModel()
            {
                CandidatePositions = candidatePositions,
                Candidates = candidates,
                PositionVoteCount = positionVoteCount,
                CandidateVoteCount = candidateVoteCount,
                TotalVotes = votes,
                TotalVoters = totalVoters
            };

            ViewBag.StartDateTime = voteEvent.StartDateTime;
            ViewBag.EndDateTime = voteEvent?.EndDateTime;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCandidates()
        {
            VoteEvent? voteEvent = this._context.VoteEvents.FirstOrDefault();

            if (voteEvent == null)
            {
                TempData["ErrorMessage"] = "No event has been scheduled";
                return RedirectToAction("Index", "Home");
            }

            List<CandidatePosition> candidatePositions = await this._context.CandidatePositions.ToListAsync();
            ViewBag.CandidatePositions = candidatePositions;

            List<Candidate> candidates = await this._context.Candidates.ToListAsync();

            return View(candidates);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            var userId = this._userManager.GetUserId(User);
            var user = this._context.ApplicationUsers.Include("Address").FirstOrDefault(e => e.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}