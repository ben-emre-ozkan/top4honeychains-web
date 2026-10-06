using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-65: landing counters and images
    public class LandingTests : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "zz_test_landing_" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        private class Env : IWebHostEnvironment
        {
            public Env(string root) { WebRootPath = root; WebRootFileProvider = new PhysicalFileProvider(root); }
            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; }
            public string ApplicationName { get; set; } = "test";
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
            public string ContentRootPath { get; set; } = "";
            public string EnvironmentName { get; set; } = "Test";
        }

        private void Touch(string relative)
        {
            var path = Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, new byte[] { 1 });
        }

        [Fact]
        public void Counters_are_hidden_when_all_are_zero()
        {
            Assert.False(LandingStats.Empty.Any);
            Assert.False(new LandingStats(0, 0, 0, true).Any);
            Assert.True(new LandingStats(0, 3, 0, false).Any);
        }

        [Fact]
        public void A_missing_image_is_a_placeholder()
        {
            Directory.CreateDirectory(_root);
            var image = LandingImages.Find(new Env(_root), "step-1-apiary");
            Assert.False(image.Exists);
            Assert.Empty(image.WebP);
        }

        [Fact]
        public void An_image_uses_its_webp_variants_when_present()
        {
            Touch("img/landing/step-3-lab.jpg");
            Touch("img/landing/step-3-lab-480.webp");
            Touch("img/landing/step-3-lab-960.webp");
            var image = LandingImages.Find(new Env(_root), "step-3-lab");
            Assert.Equal("/img/landing/step-3-lab.jpg", image.Jpeg);
            Assert.Equal("/img/landing/step-3-lab-480.webp 480w, /img/landing/step-3-lab-960.webp 960w", image.WebPSrcSet);
        }

        [Fact]
        public void Webp_variants_without_the_jpeg_are_ignored()
        {
            Touch("img/landing/step-2-delivery-480.webp");
            Assert.False(LandingImages.Find(new Env(_root), "step-2-delivery").Exists);
        }

        [Fact]
        public void Partner_logos_come_from_the_partners_folder()
        {
            Directory.CreateDirectory(_root);
            Assert.Empty(LandingImages.PartnerLogos(new Env(_root)));
            Touch("img/partners/b-university.svg");
            Touch("img/partners/a-institute.png");
            Touch("img/partners/readme.txt");
            Assert.Equal(new[] { "/img/partners/a-institute.png", "/img/partners/b-university.svg" }, LandingImages.PartnerLogos(new Env(_root)));
        }
    }
}
