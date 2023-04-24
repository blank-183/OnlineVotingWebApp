using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.ViewModels
{
    public class UpdateCandidatePositionViewModel
    {
        public int CandidatePositionId { get; set; }

        [Display(Name = "Candidate Position Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string CandidatePositionName { get; set; }
    }
}
