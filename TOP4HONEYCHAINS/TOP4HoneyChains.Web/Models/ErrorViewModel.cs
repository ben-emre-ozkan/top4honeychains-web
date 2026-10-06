namespace TOP4HoneyChains.Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        // HTTP status of the page: 403, 404, 500 (D-67)
        public int Code { get; set; } = 500;

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
