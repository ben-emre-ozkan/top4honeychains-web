using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string? UserName { get; set; }

        // D-78: optional (e-mail stays disconnected); sign-in details go by SMS
        [EmailAddress]
        public string? Email { get; set; }

        // D-78 (D78-SPEC 4.3): the mobile phone that receives the user name and temporary password
        public string? PhoneNumber { get; set; }

        // "Sms" (default): a temporary password by SMS; "Manual": the administrator sets an initial password.
        // Both must be changed at the first sign-in.
        public string? Delivery { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }

        // Required in admin mode; with the organization model empty means "no global role" (checked in the controller)
        public string? Role { get; set; }
    }
}
