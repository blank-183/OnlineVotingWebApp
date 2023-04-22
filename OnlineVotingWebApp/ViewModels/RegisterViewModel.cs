using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.ViewModels
{
    public class RegisterViewModel
    {
        [Display(Name = "First Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string? MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string LastName { get; set; }

        [Required]
        public string Sex { get; set; }

        [Display(Name = "Civil Status")]
        [Required]
        public string CivilStatus { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Phone Number")]
        [Required]
        [RegularExpression(@"^(\+639)\d{9}$", ErrorMessage = "Phone number should be in the format +639xxxxxxxxx")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Region { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string Province { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Uppercase and lowercase letters only")]
        public string Municipality { get; set; }

        [Display(Name = "Address Line")]
        [Required]
        [RegularExpression(@"^[#.0-9a-zA-Z\s,-]+$", ErrorMessage = "Special characters are not allowed")]
        public string AddressLine { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }

        //public IFormFile? Photo { get; set; }
    }
}
