using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineVotingWebApp.Models;
using OnlineVotingWebApp.Data;

namespace OnlineVotingWebApp.CustomAttributes
{
    public class VoteEventAuthorizationAttribute : ActionFilterAttribute
    {
        private readonly OnlineVotingDbContext _context;

        public VoteEventAuthorizationAttribute(OnlineVotingDbContext context)
        {
            this._context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Query the VoteEvent table using Entity Framework
            VoteEvent? voteEvent = this._context.VoteEvents.FirstOrDefault();

            // Check if an event has been scheduled or if the event has ended
            if (voteEvent == null || DateTime.UtcNow < voteEvent.StartDateTime || DateTime.UtcNow > voteEvent.EndDateTime)
            {
                // Redirect to an appropriate error page or custom access denied page
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
