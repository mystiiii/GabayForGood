using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabayForGood.DataModel
{
    public class ProjectUpdate
    {
        [Key]
        public int ProjectUpdateId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; }

        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string? ImageUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public Project Project { get; set; }
    }
}