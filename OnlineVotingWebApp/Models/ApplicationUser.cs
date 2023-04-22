using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineVotingWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(30)]
        [Unicode(false)]
        public string FirstName { get; set; } = null!;

        [StringLength(30)]
        [Unicode(false)]
        public string? MiddleName { get; set; }

        [StringLength(30)]
        [Unicode(false)]
        public string LastName { get; set; } = null!;

        [StringLength(int.MaxValue)]
        [Unicode(false)]
        public string? Photo { get; set; }

        [StringLength(1)]
        [Unicode(false)]
        public string Sex { get; set; } = null!;

        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(15)]
        [Unicode(false)]
        public string CivilStatus { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        public Address Address { get; set; } = null!;

        [NotMapped]
        public string FullName
        {
            get
            {
                if (MiddleName != null)
                {
                    return $"{FirstName} {MiddleName} {LastName}";
                }

                return FirstName + " " + LastName;
            }
        }

    }
}
