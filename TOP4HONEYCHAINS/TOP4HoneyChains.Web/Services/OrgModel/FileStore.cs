using System.Security.Cryptography;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // Documents of organizations and registry proofs: stored outside wwwroot under App_Data with a
    // random name and their SHA-256 (served only through authorized actions)
    public static class FileStore
    {
        public const string OrgDocuments = "App_Data/org-documents";
        public const string RegistryProofs = "App_Data/registry-proofs";

        public static async Task<(string Stored, string Sha256)> SaveAsync(IWebHostEnvironment env, string folder, IFormFile file)
        {
            var dir = Path.Combine(env.ContentRootPath, folder);
            Directory.CreateDirectory(dir);
            var stored = Guid.NewGuid().ToString("N") + UploadValidator.SafeDocumentExtension(file);
            await using var target = new FileStream(Path.Combine(dir, stored), FileMode.CreateNew);
            await using var source = file.OpenReadStream();
            using var sha = SHA256.Create();
            await using (var hashing = new CryptoStream(target, sha, CryptoStreamMode.Write, leaveOpen: true))
                await source.CopyToAsync(hashing);
            return (stored, Convert.ToHexString(sha.Hash!).ToLowerInvariant());
        }

        public static string? PathOf(IWebHostEnvironment env, string folder, string? stored)
        {
            if (stored == null)
                return null;
            var path = Path.Combine(env.ContentRootPath, folder, Path.GetFileName(stored));
            return File.Exists(path) ? path : null;
        }

        public static string ContentTypeOf(string path) => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }
}
