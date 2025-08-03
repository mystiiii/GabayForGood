using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabayForGood.DataModel
{
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; }

        [Required]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int YearFounded { get; set; }

        [Required]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Address { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long ContactNo { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string ContactPerson { get; set; }

        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string? OrgLink { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Project> Project { get; set; }
    }
}
