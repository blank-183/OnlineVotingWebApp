using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int VoteId { get; set; }
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public Vote Vote { get; set; } = null!;
    }
}
