using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class SignUpVM
    {
        public SignUpVM()
        {
            UserName = "";
            Password = "";
            ConfirmPassword = "";
        }

        [Required]
        [MaxLength(30)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}