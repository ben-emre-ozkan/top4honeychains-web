namespace TOP4HoneyChains.Web.Services
{
    // Absolute links that leave the site (reset e-mails, QR codes) are built from the configured
    // public address (App:PublicBaseUrl), never from the request's Host header, which a client can set.
    // Development falls back to the request so local testing works without configuration.
    public static class PublicUrl
    {
        // relativePath comes from Url.Action/Url.Content, so it already includes the PathBase
        public static string? Build(HttpContext httpContext, string relativePath)
        {
            var baseUrl = httpContext.RequestServices.GetRequiredService<IConfiguration>()["App:PublicBaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
                return baseUrl.TrimEnd('/') + relativePath;

            var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            return env.IsDevelopment()
                ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{relativePath}"
                : null;
        }
    }
}
