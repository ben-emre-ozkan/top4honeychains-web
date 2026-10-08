using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Filters;
using TOP4HoneyChains.Web.Models.Email;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Organization model (docs/ROLE-MODEL.md, P1). Features:OrgRoleModel = false keeps v1.0 behaviour.
builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection("Features"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddDbContext<TOP4HoneyChainsContext>((sp, options) => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6; // Telefon için yeterli

    // Lock an account for 15 minutes after 5 failed sign-ins (brute-force protection)
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("tr"),
    new CultureInfo("es"),
    new CultureInfo("pl"),
    new CultureInfo("lv")
};

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>, ConfigureLocalizedMvc>();

var localizationOptions = new RequestLocalizationOptions
{
    // D-51: Turkish is the default language; English is only the fallback for a missing translation
    DefaultRequestCulture = new RequestCulture("tr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

localizationOptions.RequestCultureProviders = new IRequestCultureProvider[]
{
    new QueryStringRequestCultureProvider(),
    new CookieRequestCultureProvider(),
    new AcceptLanguageHeaderRequestCultureProvider()
};


builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<CcpIntegrityService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<LandingStatsService>();
builder.Services.AddScoped<AdminHomeService>();   // D-72: the admin home
// D-73: warm the EF models, the first connections and the reference texts in the background after start-up
builder.Services.AddHostedService<StartupWarmup>();
builder.Services.AddScoped<TOP4HoneyChains.Web.Services.OrgModel.AuditReadable>();   // D-73: readable audit log
// D-73: compress the static text assets (CSS, JavaScript, SVG). HTML pages stay uncompressed: they carry antiforgery
// tokens next to user input (BREACH); IIS may compress them separately if the university IT enables it.
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    o.MimeTypes = new[] { "text/css", "text/javascript", "application/javascript", "image/svg+xml" };
});
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.AddScoped<LimitFormatter>();
builder.Services.AddSingleton<RefText>();
builder.Services.AddSingleton<Places>();
builder.Services.AddSingleton<TOP4HoneyChains.Web.Services.OrgModel.Permissions>();
builder.Services.AddScoped<MessageTranslator>();
builder.Services.AddScoped<ICurrentOrg, CurrentOrg>();
builder.Services.AddScoped<CcpResponsibilityService>();
builder.Services.AddScoped<OrgScope>();
builder.Services.AddScoped<CustodyService>();
builder.Services.AddScoped<LabService>();
builder.Services.AddScoped<StorefrontService>();
builder.Services.AddScoped<StorefrontPage>();
builder.Services.AddScoped<MyWorkService>();
builder.Services.AddScoped<ISmsSender, LoggingSmsSender>();
builder.Services.AddScoped<AuditWriter>();
builder.Services.AddScoped<ManualRegistryVerifier>();
builder.Services.AddScoped<IRegistryVerifier>(sp => sp.GetRequiredService<ManualRegistryVerifier>());
builder.Services.AddScoped<IAuthorizationHandler, PanelUserHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OrganizationAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CcpAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.PanelUser, p => p.RequireAuthenticatedUser().AddRequirements(new PanelUserRequirement()));
    options.AddPolicy(AuthPolicies.PlatformAdmin, p => p.RequireRole(CurrentOrg.AdminRole));
    options.AddPolicy(AuthPolicies.SignedIn, p => p.RequireAuthenticatedUser());
    // D-47: safety net - an endpoint without an explicit policy or [AllowAnonymous] requires sign-in
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddControllersWithViews(options =>
{
    // Every unsafe request (POST/PUT/DELETE) must carry the anti-forgery token
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());

    // FK/unique violations on save/delete -> friendly message instead of HTTP 500
    options.Filters.Add<DbUpdateExceptionFilter>();

    // Server messages are written in English and translated at display time (English stays the fallback)
    options.Filters.Add<TranslateModelStateFilter>();

    // An empty number/date field posts "" and used to show "The value '' is invalid."
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "This field is required.");

    // Decimal input must not depend on the UI language: "1.5" and "1,5" both mean 1.5 (ENG-07)
    options.ModelBinderProviders.Insert(0, new TOP4HoneyChains.Web.ModelBinding.FlexibleDecimalModelBinderProvider());
})
// Validation attribute messages are looked up in SharedResource by their English text
.AddDataAnnotationsLocalization(o => o.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(TOP4HoneyChains.Web.SharedResource)));

// D-67: errors also go to App_Data/logs, with the reference the error page shows
builder.Logging.AddProvider(new ErrorFileLoggerProvider(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "logs")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
}
// D-67: branded pages for 404 and other status codes without a body; unhandled exceptions are logged with a reference
app.UseStatusCodePagesWithReExecute("/Dashboard/Status", "?code={0}");
app.UseMiddleware<UnhandledExceptionLoggingMiddleware>();
// D-13: TLS ends at IIS (certificate and binding by the university IT); the app redirects HTTP to HTTPS
// and sends HSTS in Production only. Under IIS the HTTPS port comes from the binding (ANCM_HTTPS_PORT).
if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseResponseCompression();

// D-70: AVIF landing images are not in the default content-type map; without it they fall through to the sign-in redirect
{
    var staticTypes = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    staticTypes.Mappings[".avif"] = "image/avif";
    app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = staticTypes });
}

app.UseRouting();
{
    var refText = app.Services.GetRequiredService<RefText>();
    Origin.Translate = (code, name) => refText.Country(code, name);
}
app.UseRequestLocalization(localizationOptions);
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
// Lets the integration tests host the app (WebApplicationFactory<Program>)
public partial class Program { }
