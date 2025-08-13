using System.ComponentModel.DataAnnotations;
namespace GabayForGood.WebApp.Models
{
    public class DonationVM
    {
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Donation amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Donation amount must be greater than 0.")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "Payment method is required.")]
        public string PaymentMethod { get; set; }
        [StringLength(500, ErrorMessage = "Message must be less than 500 characters.")]
        public string? Message { get; set; }

        // Project details for display
        public ProjectVM Project { get; set; }

        // Organization details for display  
        public OrgVM Organization { get; set; }

        // Additional calculated fields for display
        public decimal CurrentAmount { get; set; }
        public decimal FundingPercentage { get; set; }
        public int DaysRemaining { get; set; }

        public DonationVM()
        {
            Project = new ProjectVM();
            Organization = new OrgVM();
        }
    }
}