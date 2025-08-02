using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabayForGood.DataModel
{
    public class Project
    {
        [Key]
        public string ProjectId { get; set; }
        public string OrganizationId { get; set; }
        public string Title{ get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Cause { get; set; }
        public decimal GoalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Organization Organization { get; set; }
    }
}
