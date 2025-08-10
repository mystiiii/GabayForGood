using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class SignInVM
    {
        public SignInVM()
        {
            Email = "";
            Password = "";
            ReturnUrl = "";
        }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
