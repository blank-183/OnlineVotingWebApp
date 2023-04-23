using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineVotingWebApp.Controllers
{
    [Authorize(Roles = "Voter")]
    public class VoterController : Controller
    {
        [HttpGet]
        public IActionResult ViewCandidates()
        {
            return View();
        }
    }
}
