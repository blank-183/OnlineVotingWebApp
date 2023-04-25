using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineVotingWebApp.ViewModels
{
    public class UpdateEventViewModel
    {
        public int VoteEventId { get; set; }

        [Required]
        [Display(Name = "Start Date Time")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "End Date Time")]
        public DateTime EndDateTime { get; set; }
    }
}
