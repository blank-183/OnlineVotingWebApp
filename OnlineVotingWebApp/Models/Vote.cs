using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace OnlineVotingWebApp.Models
{
    public class Vote
    {
        [Key]
        public int VoteId { get; set; }

        public string VoterId { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime VotedAt { get; set; } = DateTime.Now;

        public ApplicationUser ApplicationUser { get; set; } = null!;

        public ICollection<Transaction> Transactions { get; set; }
    }
}
