using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.Models
{
    public class Candidate
    {
        [Key]
        public int CandidateId { get; set; }

        public int? CandidatePositionId { get; set; } = null!;

        [StringLength(30)]
        [Unicode(false)]
        public string FirstName { get; set; } = null!;

        [StringLength(30)]
        [Unicode(false)]
        public string? MiddleName { get; set; }

        [StringLength(30)]
        [Unicode(false)]
        public string LastName { get; set; } = null!;

        [StringLength(100)]
        [Unicode(false)]
        public string Party { get; set; } = null!;

        [StringLength(int.MaxValue)]
        [Unicode(false)]
        public string? Photo { get; set; }

        public CandidatePosition CandidatePosition { get; set; }
    }
}
