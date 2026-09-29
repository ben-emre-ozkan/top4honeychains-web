using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
