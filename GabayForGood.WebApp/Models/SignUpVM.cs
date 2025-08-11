using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class SignUpVM
    {
        public SignUpVM()
        {
            FirstName = "";
            LastName = "";
            Email = "";
            ContactNo = "";
            Password = "";
            ConfirmPassword = "";
        }

        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Contact No.")]
        public string ContactNo { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}