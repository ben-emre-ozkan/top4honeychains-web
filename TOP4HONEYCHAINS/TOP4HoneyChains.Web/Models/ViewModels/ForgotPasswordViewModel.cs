using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
