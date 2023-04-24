using OnlineVotingWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.ViewModels
{
    public class UpdateCandidateViewModel
    {
        public int CandidateId { get; set; }

        [Required]
        [Display(Name = "Candidate Position")]
        public int CandidatePositionId { get; set;}

        [Display(Name = "First Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string FirstName { get; set; } = null!;

        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string? MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Last Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string Party { get; set; } = null!;

        public IFormFile? Photo { get; set; }

    }
}
