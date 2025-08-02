using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabayForGood.DataModel
{
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }
        public string Description { get; set; }
        public int YearFounded { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public int ContactNo { get; set; }
        public string ContactPerson { get; set; }
        public string? OrgLink { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Project> Project { get; set; }

    }
}
