using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GabayForGood.WebApp.Models
{
    public class ProjectUpdatesVM
    {
        public ProjectVM Project { get; set; }
        public List<ProjectUpdateVM> Updates { get; set; }
    }

    public class ProjectUpdateVM
    {
        public int ProjectUpdateId { get; set; }
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Update Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Update Description")]
        public string Description { get; set; }

        [StringLength(500, ErrorMessage = "Image URL must be less than 500 characters.")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Update Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Date Posted")]
        public DateTime CreatedAt { get; set; }
    }
}