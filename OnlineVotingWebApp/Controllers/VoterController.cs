using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineVotingWebApp.Data;
using OnlineVotingWebApp.Models;

namespace OnlineVotingWebApp.Controllers
{
    [Authorize(Roles = "Voter")]
    public class VoterController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly OnlineVotingDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public VoterController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, OnlineVotingDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult ViewCandidates()
        {
            List<CandidatePosition> candidatePositions = this._context.CandidatePositions.ToList();
            ViewBag.CandidatePositions = candidatePositions;
            
            List<Candidate> candidates = this._context.Candidates.ToList();

            return View(candidates);
        }
    }
}
