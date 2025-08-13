using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GabayForGood.DataModel
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }
        public int OrganizationId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; }

        [Required]
        [MaxLength(150)]
        [Column(TypeName = "nvarchar(150)")]
        public string Location { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Category { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Cause { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GoalAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAmount { get; set; } = 0;
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string ImageUrl { get; set; }
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Status { get; set; }



        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Organization Organization { get; set; }
        public List<ProjectUpdate> ProjectUpdates { get; set; }
        public List<Donation> Donations { get; set; }
    }
}