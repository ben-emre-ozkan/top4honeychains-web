using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.UsersManagement
{
    public class ChangePasswordViewModel
    {
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string? ConfirmPassword { get; set; }
    }
}
