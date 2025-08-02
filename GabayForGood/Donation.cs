using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabayForGood.DataModel
{
    public class Donation
    {
        [Key]
        public int DonationId { get; set; }
        public string UserId { get; set; }
        public int ProjectId { get; set; }
        public decimal Amount { get; set; }
        public string Payment { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public Project Project { get; set; }
    }

}
