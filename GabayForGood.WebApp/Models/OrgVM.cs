using Microsoft.AspNetCore.Mvc;

namespace GabayForGood.WebApp.Models
{
    public class OrgVM
    {
        public int OrganizationID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int YearFounded { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public long ContactNo { get; set; }
        public string ContactPerson { get; set; }
        public string? OrgLink { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

    }
}
