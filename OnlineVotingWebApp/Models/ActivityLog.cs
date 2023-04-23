using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineVotingWebApp.Models
{
    public class ActivityLog
    {
        [Key]
        public int ActivityLogId { get; set; }

        public string UserId { get; set; } = null!;

        [StringLength(200)]
        [Unicode(false)]
        public string Description { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime ActivityDateTime { get; set; } = DateTime.Now;

        public ApplicationUser ApplicationUser { get; set; }
    }
}
