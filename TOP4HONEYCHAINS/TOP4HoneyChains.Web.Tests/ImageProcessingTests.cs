using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // The image checks after the ImageSharp 3.2.0 update (security fixes): uploads are recognised by their content
    public class ImageProcessingTests
    {
        public static byte[] Encode(int width, int height, string format)
        {
            using var image = new Image<Rgba32>(width, height, new Rgba32(240, 180, 60));
            using var ms = new MemoryStream();
            switch (format)
            {
                case "png": image.Save(ms, new PngEncoder()); break;
                case "webp": image.Save(ms, new WebpEncoder()); break;
                default: image.Save(ms, new JpegEncoder()); break;
            }
            return ms.ToArray();
        }

        private static IFormFile File(byte[] bytes, string name) => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);

        [Theory]
        [InlineData("jpg")]
        [InlineData("png")]
        [InlineData("webp")]
        public async Task Real_images_are_accepted(string format) =>
            Assert.Null(await UploadValidator.ValidateImageAsync(File(Encode(640, 480, format), "zz_test." + format)));

        [Fact]
        public async Task A_renamed_non_image_and_an_oversized_image_are_refused()
        {
            Assert.NotNull(await UploadValidator.ValidateImageAsync(File("%PDF-1.4 zz_test"u8.ToArray(), "zz_test.jpg")));
            Assert.NotNull(await UploadValidator.ValidateImageAsync(File(Encode(640, 480, "png"), "zz_test.gif")));
            Assert.Contains("dimensions", await UploadValidator.ValidateImageAsync(File(Encode(10001, 10, "png"), "zz_test.png")));
        }
    }
}
