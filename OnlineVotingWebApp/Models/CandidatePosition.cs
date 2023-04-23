using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.Models
{
    public class CandidatePosition
    {
        [Key]
        public int CandidatePositionId { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string CandidatePositionName { get; set; } = null!;

        public ICollection<Candidate> Candidates { get; set; }
    }
}
