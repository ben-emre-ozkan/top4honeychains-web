using System.Net;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // ImageSharp 3.2.0 (security update): the uploads that resize images still work end to end
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class ImageUploadTests
    {
        private readonly E2EApp _app;
        public ImageUploadTests(E2EApp app) => _app = app;

        [Fact]
        public async Task A_beekeeper_photo_is_cropped_and_its_thumbnail_is_96_px()
        {
            await _app.CreateUserAsync("zz_test_img_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_img_admin");
            var created = await admin.PostMultipartAsync("/Beekeepers/Create", new()
            {
                ["BeekeeperFirstName"] = "Zz Image", ["BeekeeperLastName"] = "Zz Test", ["BeekeeperBirthDate"] = "1975-03-02", ["BeekeeperPhoneNumber"] = "+905320000501"
            }, ("ProfilePhotoFile", "zz_test.png", ImageProcessingTests.Encode(1200, 900, "png"), "image/png"), "/Beekeepers/Create");
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);

            var (id, photo) = await _app.WithContextAsync(db => db.Beekeepers.Where(b => b.BeekeeperFirstName == "Zz Image")
                .Select(b => new { b.BeekeeperId, b.BeekeeperProfilePhoto }).FirstAsync().ContinueWith(t => (t.Result.BeekeeperId!, t.Result.BeekeeperProfilePhoto!)));
            using (var stored = Image.Load(photo))
            {
                Assert.Equal(300, stored.Width);
                Assert.Equal(300, stored.Height);
            }
            var thumb = await admin.GetAsync($"/Beekeepers/Thumb?id={id}&v=1");
            Assert.Equal("image/jpeg", thumb.Content.Headers.ContentType?.MediaType);
            using var small = Image.Load(await thumb.Content.ReadAsByteArrayAsync());
            Assert.Equal(96, small.Width);
            Assert.Equal(96, small.Height);
        }

        [Fact]
        public async Task An_apiary_photo_is_resized_to_800_by_530_jpeg()
        {
            await _app.CreateUserAsync("zz_test_img_admin2", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_img_admin2");
            var (apiaryId, periodId) = await _app.WithContextAsync(async db =>
            {
                var bk = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "Zz", BeekeeperLastName = "Photo", BeekeeperBirthDate = new DateOnly(1980, 1, 1), BeekeeperBusinessNumber = "TR-BEE-2026-zz0501", IsActive = true };
                db.Beekeepers.Add(bk);
                var a = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = "zz_test photo apiary", ApiaryBeehivesCount = 3, ApiaryLocationLatitude = 40.1m, ApiaryLocationLongitude = 30.1m, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(a);
                await db.SaveChangesAsync();
                return (a.ApiaryId, await db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync());
            });
            var page = $"/ApiaryPhotos/Index?apiaryId={apiaryId}&periodId={periodId}";
            var uploaded = await admin.PostMultipartAsync("/ApiaryPhotos/UploadSingle", new() { ["apiaryId"] = apiaryId.ToString(), ["periodId"] = periodId.ToString(), ["desc"] = "zz_test" },
                ("file", "zz_test.webp", ImageProcessingTests.Encode(1600, 1600, "webp"), "image/webp"), page);
            Assert.Equal(HttpStatusCode.Redirect, uploaded.StatusCode);
            var data = await _app.WithContextAsync(db => db.ApiaryPhotos.Where(p => p.ApiaryPhotoApiaryId == apiaryId).Select(p => p.ApiaryPhotoData).FirstAsync());
            using var image = Image.Load(data!);
            Assert.Equal(800, image.Width);
            Assert.Equal(530, image.Height);
            Assert.Equal("JPEG", image.Metadata.DecodedImageFormat?.Name);
        }
    }
}
