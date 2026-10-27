namespace TOP4HoneyChains.Web.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        // D-78: confirmed by an SMS code (needed for the password reset by SMS)
        public bool PhoneConfirmed { get; set; }
    }
}
