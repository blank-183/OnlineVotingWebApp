using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;
using OnlineVotingWebApp.VoterViewModels;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.CodeAnalysis.Options;
using OnlineVotingWebApp.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Identity;

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

        //[HttpGet]
        //public IActionResult VoteResults()
        //{
        //    var voteResults = _context.Candidates
        //        .Include(c => c.CandidatePosition)
        //        .GroupJoin(
        //            _context.Transactions,
        //            c => c.CandidateId,
        //            t => t.CandidateId,
        //            (c, t) => new { Candidate = c, Transactions = t })
        //        .Select(g => new VoteResultViewModel
        //        {
        //            Candidate = g.Candidate,
        //            TotalVotes = g.Transactions.Count(),
        //            Percentage = 0 // Placeholder for percentage calculation
        //        })
        //        .ToList();

        //    // Calculate total votes for each candidate's position
        //    var positionIds = voteResults.Select(v => v.Candidate.CandidatePositionId).Distinct().ToList();
        //    var positionTotalVotes = new Dictionary<int?, int>(); // Dictionary to store positionId and total votes
        //    foreach (var positionId in positionIds)
        //    {
        //        var total = _context.Transactions.Count(t => t.Candidate.CandidatePositionId == positionId);
        //        positionTotalVotes.Add(positionId, total);
        //    }

        //    // Calculate abstained votes and percentage for each candidate
        //    var totalVotes = _context.Transactions.Count();
        //    foreach (var voteResult in voteResults)
        //    {
        //        var positionId = voteResult.Candidate.CandidatePositionId;
        //        var positionTotalVoteCount = positionTotalVotes[positionId];
        //        var abstainedVotes = positionTotalVoteCount - voteResult.TotalVotes;
        //        voteResult.AbstainedVotes = abstainedVotes;

        //        if (totalVotes > 0)
        //        {
        //            voteResult.Percentage = (voteResult.TotalVotes * 100.0) / totalVotes;
        //        }
        //    }

        //    return Json(voteResults);
        //}

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