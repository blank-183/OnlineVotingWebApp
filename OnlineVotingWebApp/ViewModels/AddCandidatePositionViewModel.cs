using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.ViewModels
{
    public class AddCandidatePositionViewModel
    {
        [Display(Name = "Candidate Position Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string CandidatePositionName { get; set; }
    }
}
