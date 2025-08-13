using GabayForGood.DataModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Donation
{
    [Key]
    public int DonationId { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string PaymentMethod { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    public string Message { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

}
