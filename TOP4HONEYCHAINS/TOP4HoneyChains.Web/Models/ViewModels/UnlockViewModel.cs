using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    public class UnlockViewModel
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
