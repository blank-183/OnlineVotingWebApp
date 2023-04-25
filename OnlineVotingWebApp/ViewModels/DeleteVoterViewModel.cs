using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.ViewModels
{
    public class DeleteVoterViewModel
    {
        public string VoterId { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;
    }
}
