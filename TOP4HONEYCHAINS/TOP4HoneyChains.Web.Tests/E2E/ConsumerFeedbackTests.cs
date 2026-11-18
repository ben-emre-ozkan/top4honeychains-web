using System.Net;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-87 part 5: feedback to the producer. It reaches the organization that owns the batch, which sees the pseudonym only,
    // reads it in My work and on the list, marks it read, answers and closes it; the consumer reads the answer. Limits per batch
    // and per day; suspected fakes also reach the platform administrator (read-only); audit entries carry ids only; feedback stays
    // with the organization under the pseudonym after the consumer deletes the account.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class ConsumerFeedbackTests
    {
        private readonly E2EApp _app;

        public ConsumerFeedbackTests(E2EApp app) => _app = app;

        private async Task<UserClient> MemberAsync(int orgId, string userName, string role)
        {
            var userId = await _app.CreateUserAsync(userName);
            await _app.AddMemberAsync(orgId, userId, role);
            return await UserClient.LoginAsync(_app, userName);
        }

        private static Task<HttpResponseMessage> SendAsync(UserClient consumer, Guid qr, string category, string message) =>
            consumer.PostAsync($"/Consumer/Feedback?qr={qr}", new() { ["category"] = category, ["message"] = message }, $"/Consumer/Feedback?qr={qr}");

        private Task<List<ConsumerFeedback>> FeedbackAsync(int batchId) =>
            _app.WithContextAsync(db => db.ConsumerFeedback.AsNoTracking().Where(f => f.FinalHoneyProductionId == batchId).ToListAsync());

        [Fact]
        public async Task Feedback_reaches_the_owner_who_sees_the_pseudonym_answers_and_closes_it() => await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
        {
            var batch = await ConsumerHoneyTests.BatchAsync(_app, "fb1");
            var qa = await MemberAsync(batch.OrgId, "zz_test_fb1_qa", OrgRoles.QualityApprover);
            var staff = await MemberAsync(batch.OrgId, "zz_test_fb1_staff", OrgRoles.Staff);
            var (consumer, _) = await ConsumerAccountTests.SignUpAsync(_app, "zz_test_fb1_cons");

            var sent = await SendAsync(consumer, batch.Qr, ConsumerFeedbackCategory.TasteSmell, "zz_test the taste is lovely\nsecond line");
            Assert.Equal("/Consumer", sent.Headers.Location!.ToString());
            var feedback = Assert.Single(await FeedbackAsync(batch.BatchId));
            Assert.Equal(batch.OrgId, feedback.OrgId);
            Assert.Matches("^C-[A-Z2-9]{6}$", feedback.Pseudonym);

            // My work and the list of the organization: the pseudonym, never the user name
            var work = await qa.HtmlAsync("/MyWork/Index");
            Assert.Contains($"/ConsumerFeedback/Details/{feedback.FeedbackId}", work);
            var list = await qa.HtmlAsync("/ConsumerFeedback");
            Assert.Contains(feedback.Pseudonym, list);
            Assert.DoesNotContain("zz_test_fb1_cons", list);
            Assert.DoesNotContain("zz_test_fb1_cons", await qa.HtmlAsync($"/ConsumerFeedback/Details/{feedback.FeedbackId}"));
            // a role without the right has no list and no item
            Assert.Equal(404, await staff.StatusAsync("/ConsumerFeedback"));
            Assert.DoesNotContain("/ConsumerFeedback/Details/", await staff.HtmlAsync("/MyWork/Index"));

            await qa.PostAsync($"/ConsumerFeedback/MarkRead/{feedback.FeedbackId}", new(), $"/ConsumerFeedback/Details/{feedback.FeedbackId}");
            Assert.Equal(ConsumerFeedbackStatus.Read, (await FeedbackAsync(batch.BatchId))[0].Status);
            await qa.PostAsync($"/ConsumerFeedback/Answer/{feedback.FeedbackId}", new() { ["answer"] = "zz_test thank you" }, $"/ConsumerFeedback/Details/{feedback.FeedbackId}");
            Assert.Equal(ConsumerFeedbackStatus.Answered, (await FeedbackAsync(batch.BatchId))[0].Status);
            Assert.Contains("zz_test thank you", await consumer.HtmlAsync("/Consumer"));
            // answered feedback leaves My work
            Assert.DoesNotContain($"/ConsumerFeedback/Details/{feedback.FeedbackId}", await qa.HtmlAsync("/MyWork/Index"));
            await qa.PostAsync($"/ConsumerFeedback/Close/{feedback.FeedbackId}", new(), $"/ConsumerFeedback/Details/{feedback.FeedbackId}");
            Assert.Equal(ConsumerFeedbackStatus.Closed, (await FeedbackAsync(batch.BatchId))[0].Status);

            // audit entries: created, answered, closed, with ids only (no message, no answer text, no user name)
            var audit = await _app.WithContextAsync(db => db.AuditEntries.AsNoTracking()
                .Where(a => a.Entity == "ConsumerFeedback" && a.EntityKey == feedback.FeedbackId.ToString()).ToListAsync());
            Assert.Equal(new[] { "Answer", "Close", "Create" }, audit.Select(a => a.Action).OrderBy(a => a).ToArray());
            Assert.All(audit, a => Assert.DoesNotContain("zz_test", (a.OldValues ?? "") + (a.NewValues ?? "")));
            Assert.All(audit, a => Assert.NotEqual("zz_test_fb1_cons", a.UserName));
            return 0;
        });

        [Fact]
        public async Task Limits_one_per_batch_per_day_five_per_day_1000_characters_and_known_categories() => await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
        {
            var batches = new List<ConsumerHoneyTests.TestBatch>();
            for (var i = 0; i < 6; i++)
                batches.Add(await ConsumerHoneyTests.BatchAsync(_app, $"fbl{i}"));
            var (consumer, _) = await ConsumerAccountTests.SignUpAsync(_app, "zz_test_fbl_cons");
            await SendAsync(consumer, batches[0].Qr, ConsumerFeedbackCategory.Other, new string('x', 1001));
            await SendAsync(consumer, batches[0].Qr, "Rating", "zz_test five stars");
            Assert.Empty(await FeedbackAsync(batches[0].BatchId));
            await SendAsync(consumer, batches[0].Qr, ConsumerFeedbackCategory.Other, "zz_test one");
            await SendAsync(consumer, batches[0].Qr, ConsumerFeedbackCategory.Other, "zz_test again");
            Assert.Single(await FeedbackAsync(batches[0].BatchId));
            for (var i = 1; i < 6; i++)
                await SendAsync(consumer, batches[i].Qr, ConsumerFeedbackCategory.Other, $"zz_test {i}");
            Assert.Equal(5, await _app.WithContextAsync(db => db.ConsumerFeedback.CountAsync(f => batches.Select(b => b.BatchId).Contains(f.FinalHoneyProductionId))));
            Assert.Empty(await FeedbackAsync(batches[5].BatchId));
            // the next day the limits start again
            _app.Clock.Advance(TimeSpan.FromDays(1.1));
            await SendAsync(consumer, batches[5].Qr, ConsumerFeedbackCategory.Other, "zz_test next day");
            Assert.Single(await FeedbackAsync(batches[5].BatchId));
            return 0;
        });

        [Fact]
        public async Task Suspected_fakes_reach_the_administrator_read_only_and_feedback_survives_the_account() => await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
        {
            var batch = await ConsumerHoneyTests.BatchAsync(_app, "fb2");
            var other = await ConsumerHoneyTests.BatchAsync(_app, "fb3");
            var qaOther = await MemberAsync(other.OrgId, "zz_test_fb3_qa", OrgRoles.QualityApprover);
            await _app.CreateUserAsync("zz_test_fb2_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_fb2_admin");
            var (consumer, _) = await ConsumerAccountTests.SignUpAsync(_app, "zz_test_fb2_cons");
            await SendAsync(consumer, batch.Qr, ConsumerFeedbackCategory.SuspectedFake, "zz_test the seal was broken");
            await SendAsync(consumer, other.Qr, ConsumerFeedbackCategory.PackagingLabel, "zz_test label");
            var fake = Assert.Single(await FeedbackAsync(batch.BatchId));
            var label = Assert.Single(await FeedbackAsync(other.BatchId));

            Assert.Contains($"/ConsumerFeedback/Details/{fake.FeedbackId}", await admin.HtmlAsync("/MyWork/Index"));
            Assert.Equal(200, await admin.StatusAsync($"/ConsumerFeedback/Details/{fake.FeedbackId}"));
            Assert.Equal(404, await admin.StatusAsync($"/ConsumerFeedback/Details/{label.FeedbackId}"));
            Assert.Equal(HttpStatusCode.NotFound, (await admin.PostAsync($"/ConsumerFeedback/Answer/{fake.FeedbackId}", new() { ["answer"] = "zz_test" }, "/MyWork/Index")).StatusCode);
            // another organization does not see it
            Assert.Equal(404, await qaOther.StatusAsync($"/ConsumerFeedback/Details/{fake.FeedbackId}"));

            await consumer.PostAsync("/Consumer/Delete", new() { ["password"] = E2EApp.Password }, "/Consumer/Account");
            var kept = Assert.Single(await FeedbackAsync(batch.BatchId));
            Assert.Null(kept.ConsumerUserId);
            Assert.Equal(fake.Pseudonym, kept.Pseudonym);
            Assert.Equal(200, await admin.StatusAsync($"/ConsumerFeedback/Details/{fake.FeedbackId}"));
            return 0;
        });
    }
}
