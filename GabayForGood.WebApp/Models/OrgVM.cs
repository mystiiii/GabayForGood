using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class OrgVM
    {
        public int OrganizationID { get; set; }

        [Required(ErrorMessage = "Organization Name is required.")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description must be less than 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Year Founded is required.")]
        [Range(1800, 2100, ErrorMessage = "Year Founded must be between 1800 and 2100.")]
        public int YearFounded { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        [RegularExpression(@"^(\+63|0)[0-9]{9,10}$", ErrorMessage = "Invalid Philippine contact number.")]
        public string ContactNo { get; set; }

        [Required(ErrorMessage = "Contact Person is required.")]
        public string ContactPerson { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        public string? OrgLink { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
