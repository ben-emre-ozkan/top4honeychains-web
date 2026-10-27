namespace TOP4HoneyChains.Web.Services
{
    // Server-side checks for uploaded files (type, content and size). The browser's
    // "accept" attribute and the posted Content-Type header can be forged, so the file
    // extension and the file's own signature are checked. Limits: docs/DECISIONS.md (D-10).
    public static class UploadValidator
    {
        public const long MaxImageBytes = 8 * 1024 * 1024;
        public const long MaxVideoBytes = 25 * 1024 * 1024;
        public const int MaxImageDimension = 10000;

        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly HashSet<string> ImageFormats = new(StringComparer.OrdinalIgnoreCase) { "JPEG", "PNG", "WEBP" };
        private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm" };

        public const string ImageRule = "JPG, PNG or WEBP, at most 8 MB";
        public const string VideoRule = "MP4 or WEBM, at most 25 MB";

        // Returns an error message, or null when the image is acceptable
        public static async Task<string?> ValidateImageAsync(IFormFile file)
        {
            if (file.Length == 0)
                return "The file is empty.";
            if (file.Length > MaxImageBytes)
                return $"The image is too large ({ImageRule}).";
            if (!ImageExtensions.Contains(Path.GetExtension(file.FileName)))
                return $"Unsupported image type ({ImageRule}).";

            await using var stream = file.OpenReadStream();
            using var copy = new MemoryStream();
            await stream.CopyToAsync(copy);
            copy.Position = 0;
            var info = ImageTools.Identify(copy);
            if (info == null || !ImageFormats.Contains(info.Format))
                return $"The file is not a valid image ({ImageRule}).";
            if (info.Width > MaxImageDimension || info.Height > MaxImageDimension)
                return $"The image dimensions are too large (at most {MaxImageDimension} px).";

            return null;
        }

        // Returns an error message, or null when the video is acceptable
        public static async Task<string?> ValidateVideoAsync(IFormFile file)
        {
            if (file.Length == 0)
                return "The file is empty.";
            if (file.Length > MaxVideoBytes)
                return $"The video is too large ({VideoRule}).";

            var ext = Path.GetExtension(file.FileName);
            if (!VideoExtensions.Contains(ext))
                return $"Unsupported video type ({VideoRule}).";

            var header = new byte[12];
            await using (var stream = file.OpenReadStream())
            {
                var read = 0;
                while (read < header.Length)
                {
                    var n = await stream.ReadAsync(header.AsMemory(read));
                    if (n == 0) break;
                    read += n;
                }
                if (read < header.Length)
                    return $"The file is not a valid video ({VideoRule}).";
            }

            // MP4/ISO media: "ftyp" box at offset 4. WebM/Matroska: EBML magic 1A 45 DF A3.
            var isMp4 = header[4] == (byte)'f' && header[5] == (byte)'t' && header[6] == (byte)'y' && header[7] == (byte)'p';
            var isWebm = header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3;
            var ok = ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase) ? isMp4 : isWebm;
            return ok ? null : $"The file is not a valid video ({VideoRule}).";
        }

        public const long MaxDocumentBytes = 10 * 1024 * 1024;
        public const string DocumentRule = "PDF at most 10 MB, or JPG, PNG or WEBP at most 8 MB";

        // CCP file values such as lab reports (D-19). Returns an error message, or null when acceptable
        public static async Task<string?> ValidateDocumentAsync(IFormFile file)
        {
            if (file.Length == 0)
                return "The file is empty.";

            var ext = Path.GetExtension(file.FileName);
            if (ImageExtensions.Contains(ext))
                return await ValidateImageAsync(file) is string imageError ? $"{imageError} Allowed: {DocumentRule}." : null;

            if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return $"Unsupported file type ({DocumentRule}).";
            if (file.Length > MaxDocumentBytes)
                return $"The file is too large ({DocumentRule}).";

            var header = new byte[5];
            await using (var stream = file.OpenReadStream())
            {
                var read = 0;
                while (read < header.Length)
                {
                    var n = await stream.ReadAsync(header.AsMemory(read));
                    if (n == 0) break;
                    read += n;
                }
                if (read < header.Length)
                    return $"The file is not a valid PDF ({DocumentRule}).";
            }

            // PDF files start with "%PDF-"
            var isPdf = header[0] == (byte)'%' && header[1] == (byte)'P' && header[2] == (byte)'D' && header[3] == (byte)'F' && header[4] == (byte)'-';
            return isPdf ? null : $"The file is not a valid PDF ({DocumentRule}).";
        }

        // Stored document names use a fixed, lower-case extension from the allow-list
        public static string SafeDocumentExtension(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return ext switch { ".jpeg" => ".jpg", ".jpg" or ".png" or ".webp" or ".pdf" => ext, _ => ".bin" };
        }

        // Stored video names use a fixed, lower-case extension from the allow-list
        public static string SafeVideoExtension(IFormFile file) =>
            Path.GetExtension(file.FileName).Equals(".webm", StringComparison.OrdinalIgnoreCase) ? ".webm" : ".mp4";
    }
}
