using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class AdminVM
    {
        public AdminVM()
        {
            Username = "";
            Password = "";
            ReturnURL = "";
        }

        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? ReturnURL { get; set; }
    }
}
