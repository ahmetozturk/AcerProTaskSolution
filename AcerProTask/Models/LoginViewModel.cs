using System.ComponentModel.DataAnnotations;

namespace AcerProTask.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or username is required.")]
        [StringLength(100, ErrorMessage = "Email or username cannot be longer than 100 characters.")]
        public string EmailorUserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
    }


}
