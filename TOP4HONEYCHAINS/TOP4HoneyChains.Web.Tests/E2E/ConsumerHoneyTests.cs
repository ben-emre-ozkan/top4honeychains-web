using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-87 part 4: the public Honey Story stays the same for everyone (nothing user-specific can reach a shared cache); the
    // consumer's state comes from a private no-store endpoint; saving is explicit and only for finalized batches, by QR id.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class ConsumerHoneyTests
    {
        private readonly E2EApp _app;

        public ConsumerHoneyTests(E2EApp app) => _app = app;

        public record TestBatch(int OrgId, int BatchId, Guid Qr);

        // a batch owned by a processing organization (finalized unless asked otherwise)
        public static async Task<TestBatch> BatchAsync(E2EApp app, string tag, bool finalized = true)
        {
            var wholesalerId = await app.WithContextAsync(async db =>
            {
                var w = new Wholesaler { WholesalerTitle = $"zz_test {tag} plant", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(w);
                await db.SaveChangesAsync();
                return w.WholesalerId;
            });
            var orgId = await app.CreateOrgAsync($"zz_test {tag} plant", wholesalerId, (Capabilities.Processing, null));
            var qr = Guid.NewGuid();
            var batchId = await app.WithContextAsync(async db =>
            {
                var b = new FinalHoneyProduction
                {
                    FinalHoneyProductionTitle = $"zz_test {tag} honey", FinalHoneyProductionQR = qr, FinalHoneyProductionDate = DateTime.Today,
                    FinalHoneyProductionWholesalerId = wholesalerId, OwnerOrgId = orgId, FinalHoneyProductionIsFinalized = finalized,
                    FinalHoneyProductionFinalizedDate = finalized ? DateTime.Today : null
                };
                db.FinalHoneyProductions.Add(b);
                await db.SaveChangesAsync();
                return b.FinalHoneyProductionId;
            });
            return new TestBatch(orgId, batchId, qr);
        }

        [Fact]
        public async Task The_honey_story_is_the_same_for_everyone_and_the_consumer_state_is_private() => await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
        {
            var batch = await BatchAsync(_app, "hs1");
            var anonymous = UserClient.Anonymous(_app);
            var anonResponse = await anonymous.GetAsync($"/honeystory/{batch.Qr}");
            Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);
            Assert.False(anonResponse.Headers.Contains("Set-Cookie"));
            var anonHtml = await anonResponse.Content.ReadAsStringAsync();
            // links to sign in or create an account that come back to the same story
            var back = Uri.EscapeDataString($"/honeystory/{batch.Qr}");
            Assert.Contains($"/Account/Login?ReturnUrl={back}", anonHtml);
            Assert.Contains($"/Consumer/SignUp?returnUrl={back}", anonHtml);
            Assert.Contains($"/Consumer/Save?qr={batch.Qr}", anonHtml);
            Assert.Contains("consumer-story.js", anonHtml);

            var (consumer, _) = await ConsumerAccountTests.SignUpAsync(_app, "zz_test_hs_cons1");
            var consumerResponse = await consumer.GetAsync($"/honeystory/{batch.Qr}");
            Assert.False(consumerResponse.Headers.Contains("Set-Cookie"));
            Assert.Equal(Normalize(anonHtml), Normalize(await consumerResponse.Content.ReadAsStringAsync()));
            Assert.DoesNotContain("zz_test_hs_cons1", await consumerResponse.Content.ReadAsStringAsync());

            // the private state endpoint
            var anonState = await anonymous.GetAsync($"/Consumer/State?qr={batch.Qr}");
            Assert.Contains("no-store", anonState.Headers.CacheControl!.ToString());
            Assert.Contains("private", anonState.Headers.CacheControl!.ToString());
            Assert.False(JsonDocument.Parse(await anonState.Content.ReadAsStringAsync()).RootElement.GetProperty("signedIn").GetBoolean());
            using (var state = JsonDocument.Parse(await (await consumer.GetAsync($"/Consumer/State?qr={batch.Qr}")).Content.ReadAsStringAsync()))
            {
                Assert.True(state.RootElement.GetProperty("signedIn").GetBoolean());
                Assert.False(state.RootElement.GetProperty("saved").GetBoolean());
            }
            // opening the story recorded nothing
            Assert.Equal(0, await _app.WithContextAsync(db => db.ConsumerSavedBatches.CountAsync(s => s.FinalHoneyProductionId == batch.BatchId)));
            return 0;
        });

        // the page carries asp-append-version hashes; nothing else may differ
        private static string Normalize(string html) => Regex.Replace(html, @"\?v=[A-Za-z0-9_-]+", "");

        [Fact]
        public async Task Saving_is_explicit_only_for_finalized_batches_and_removable() => await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
        {
            var batch = await BatchAsync(_app, "hs2");
            var draft = await BatchAsync(_app, "hs3", finalized: false);
            var (consumer, _) = await ConsumerAccountTests.SignUpAsync(_app, "zz_test_hs_cons2");
            // the confirmation page saves nothing
            Assert.Contains(batch.Qr.ToString(), await consumer.HtmlAsync($"/Consumer/Save?qr={batch.Qr}"));
            Assert.Equal(0, await _app.WithContextAsync(db => db.ConsumerSavedBatches.CountAsync(s => s.FinalHoneyProductionId == batch.BatchId)));
            await consumer.PostAsync($"/Consumer/Save?qr={batch.Qr}", new(), $"/Consumer/Save?qr={batch.Qr}");
            var home = await consumer.HtmlAsync("/Consumer");
            Assert.Contains("zz_test hs2 honey", home);
            Assert.Contains("zz_test hs2 plant", home);
            Assert.True(JsonDocument.Parse(await (await consumer.GetAsync($"/Consumer/State?qr={batch.Qr}")).Content.ReadAsStringAsync()).RootElement.GetProperty("saved").GetBoolean());
            // a batch that is not finalized has no story and cannot be saved
            Assert.Equal(404, await consumer.StatusAsync($"/Consumer/Save?qr={draft.Qr}"));
            Assert.Equal(HttpStatusCode.NotFound, (await consumer.PostAsync($"/Consumer/Save?qr={draft.Qr}", new(), "/Consumer")).StatusCode);
            // remove
            await consumer.PostAsync($"/Consumer/Remove?qr={batch.Qr}", new(), "/Consumer");
            Assert.DoesNotContain("zz_test hs2 honey", await consumer.HtmlAsync("/Consumer"));
            return 0;
        });

        [Fact]
        public async Task An_anonymous_save_goes_to_sign_in_and_with_the_switch_off_the_story_has_no_consumer_block()
        {
            var batch = await BatchAsync(_app, "hs4");
            await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
            {
                var save = await UserClient.Anonymous(_app).GetAsync($"/Consumer/Save?qr={batch.Qr}");
                Assert.Equal(HttpStatusCode.Redirect, save.StatusCode);
                Assert.Contains("/Account/Login", save.Headers.Location!.ToString());
                return 0;
            });
            var html = await UserClient.Anonymous(_app).HtmlAsync($"/honeystory/{batch.Qr}");
            Assert.DoesNotContain("consumer-actions", html);
            Assert.DoesNotContain("consumer-story.js", html);
        }
    }
}
