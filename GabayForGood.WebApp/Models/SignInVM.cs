using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class SignInVM
    {
        public SignInVM()
        {
            UserName = "";
            Password = "";
            ReturnUrl = "";
        }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
