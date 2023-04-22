using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.Models
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        public string UserId { get; set; } = null!;

        [StringLength(15)]
        [Unicode(false)]
        public string Region { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string Province { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string Municipality { get; set; } = null!;

        [StringLength(200)]
        [Unicode(false)]
        public string AddressLine { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
