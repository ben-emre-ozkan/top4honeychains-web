using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-108: the parameter videos of the Honey Story: known keys and languages only, the fallback page language -> en -> tr,
    // and the import tool knows the same keys
    public class ParameterVideosTests : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "t4h-videos-" + Guid.NewGuid().ToString("N"));

        private class Env : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = "";
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
            public string ApplicationName { get; set; } = "test";
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public string EnvironmentName { get; set; } = "Test";
        }

        private ParameterVideos Videos() => new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Videos:Root"] = _root }).Build(), new Env());

        private void Put(string lang, string key)
        {
            Directory.CreateDirectory(Path.Combine(_root, lang));
            File.WriteAllBytes(Path.Combine(_root, lang, key + ".mp4"), new byte[] { 0 });
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        [Fact]
        public void Parameter_names_map_to_their_keys()
        {
            Assert.Equal("hmf", ParameterVideos.KeyOf("HMF"));
            Assert.Equal("fructose-glucose", ParameterVideos.KeyOf("Fruktoz"));
            Assert.Equal("fructose-glucose", ParameterVideos.KeyOf(" glikoz "));
            Assert.Equal("c4-sugar", ParameterVideos.KeyOf("C4 Sugar"));
            Assert.Null(ParameterVideos.KeyOf("Sensory Analysis"));
            Assert.Null(ParameterVideos.KeyOf(null));
        }

        [Fact]
        public void The_page_language_comes_first_then_English_then_Turkish()
        {
            Put("tr", "hmf");
            var videos = Videos();
            Assert.Equal("tr", videos.LanguageFor("hmf", "pl"));
            Put("en", "hmf");
            Assert.Equal("en", videos.LanguageFor("hmf", "pl"));
            Assert.Equal("tr", videos.LanguageFor("hmf", "tr"));
            Put("pl", "hmf");
            Assert.Equal("pl", videos.LanguageFor("hmf", "pl"));
            Assert.Null(videos.LanguageFor("moisture", "en"));   // no file: the sheet shows the text only
        }

        [Fact]
        public void Only_known_keys_and_languages_reach_the_disk()
        {
            Put("en", "hmf");
            File.WriteAllBytes(Path.Combine(_root, "secret.mp4"), new byte[] { 0 });
            var videos = Videos();
            Assert.NotNull(videos.File("en", "hmf"));
            Assert.Null(videos.File("xx", "hmf"));
            Assert.Null(videos.File("en", "../secret"));
            Assert.Null(videos.File("..", "secret"));
            Assert.Null(videos.File("en", "unknown"));
        }

        [Fact]
        public void The_import_tool_knows_every_key()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "tools", "ImportParameterVideos.ps1")))
                dir = dir.Parent;
            var script = File.ReadAllText(Path.Combine(dir!.FullName, "tools", "ImportParameterVideos.ps1"));
            foreach (var key in ParameterVideos.Keys.Keys)
                Assert.Contains($"'{key}'", script);
            foreach (var lang in ParameterVideos.Languages)
                Assert.Contains($"'{lang}'", script);
        }
    }
}
