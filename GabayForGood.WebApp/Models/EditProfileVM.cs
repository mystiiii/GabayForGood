using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class EditProfileVM
    {
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
    }
}
