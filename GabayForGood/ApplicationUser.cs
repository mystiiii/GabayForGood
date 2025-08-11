using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabayForGood.DataModel
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }    
        public string? FirstName { get; set; } 
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string? ContactNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? OrganizationID { get; set; }

        public List<Donation> Donation { get; set; }

        [ForeignKey("OrganizationID")]
        public Organization Organization { get; set; }
    }
}
