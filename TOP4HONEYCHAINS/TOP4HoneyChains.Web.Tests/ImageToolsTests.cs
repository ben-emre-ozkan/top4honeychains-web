using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-85: the SkiaSharp image helper (ImageSharp only builds the fixtures and the expected result here)
    public class ImageToolsTests
    {
        // a 60 × 30 picture in four coloured quadrants, stored with the given EXIF orientation
        private static byte[] Quadrants(ushort orientation)
        {
            using var image = new Image<Rgba32>(60, 30);
            image.ProcessPixelRows(rows =>
            {
                for (var y = 0; y < rows.Height; y++)
                {
                    var row = rows.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                        row[x] = (x < 30, y < 15) switch
                        {
                            (true, true) => new Rgba32(220, 20, 20),
                            (false, true) => new Rgba32(20, 20, 220),
                            (true, false) => new Rgba32(20, 200, 20),
                            _ => new Rgba32(230, 220, 20)
                        };
                }
            });
            image.Metadata.ExifProfile = new ExifProfile();
            image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, orientation);
            image.Metadata.ExifProfile.SetValue(ExifTag.Software, "zz_test camera");
            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 95 });
            return ms.ToArray();
        }

        private static bool Close(Rgba32 a, Rgba32 b) => Math.Abs(a.R - b.R) < 60 && Math.Abs(a.G - b.G) < 60 && Math.Abs(a.B - b.B) < 60;

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
        public void Every_exif_orientation_is_turned_upright_and_no_metadata_is_kept(ushort orientation)
        {
            var source = Quadrants(orientation);
            using var expected = Image.Load<Rgba32>(source);
            expected.Mutate(x => x.AutoOrient());

            var clean = ImageTools.CleanJpeg(source, 90);
            Assert.NotNull(clean);
            using var actual = Image.Load<Rgba32>(clean!);
            Assert.Equal(expected.Width, actual.Width);
            Assert.Equal(expected.Height, actual.Height);
            foreach (var (fx, fy) in new[] { (0.25, 0.25), (0.75, 0.25), (0.25, 0.75), (0.75, 0.75) })
            {
                int x = (int)(actual.Width * fx), y = (int)(actual.Height * fy);
                Assert.True(Close(expected[x, y], actual[x, y]), $"orientation {orientation} at {x},{y}: expected {expected[x, y]}, got {actual[x, y]}");
            }
            Assert.Null(actual.Metadata.ExifProfile);
            Assert.Null(actual.Metadata.XmpProfile);
            Assert.Null(actual.Metadata.IptcProfile);
        }

        [Fact]
        public void Crop_fills_the_target_size_around_the_centre()
        {
            var jpeg = ImageTools.CropToJpeg(Quadrants(1), 20, 20, 85);
            using var image = Image.Load<Rgba32>(jpeg!);
            Assert.Equal(20, image.Width);
            Assert.Equal(20, image.Height);
            // the centre square of the 60 × 30 picture: the left half red over green, the right half blue over yellow
            Assert.True(Close(new Rgba32(220, 20, 20), image[4, 4]));
            Assert.True(Close(new Rgba32(230, 220, 20), image[15, 15]));
            Assert.Null(image.Metadata.ExifProfile);
        }

        [Fact]
        public void Identify_reads_the_format_from_the_content_and_refuses_other_bytes()
        {
            using var stream = new MemoryStream(Quadrants(1));
            var info = ImageTools.Identify(stream);
            Assert.Equal(new ImageTools.Info("JPEG", 60, 30), info);
            Assert.Null(ImageTools.Identify(new MemoryStream("%PDF-1.4 zz_test"u8.ToArray())));
            Assert.Null(ImageTools.CleanJpeg("not an image"u8.ToArray(), 85));
        }
    }
}
