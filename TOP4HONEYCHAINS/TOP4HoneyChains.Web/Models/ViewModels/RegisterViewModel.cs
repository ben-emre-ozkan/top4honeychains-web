using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }

        // Required in admin mode; with the organization model empty means "no global role" (checked in the controller)
        public string? Role { get; set; }
    }
}
