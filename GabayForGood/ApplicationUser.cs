using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabayForGood.DataModel
{
    public class ApplicationUser : IdentityUser
    {
        // New Fields
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNo { get; set; }

        // Keep existing
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Donation> Donation { get; set; }
    }
}
