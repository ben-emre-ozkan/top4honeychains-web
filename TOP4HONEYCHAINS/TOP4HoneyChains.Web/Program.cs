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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<TOP4HoneyChainsContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
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

builder.Services.AddControllersWithViews(options =>
{
    // Every unsafe request (POST/PUT/DELETE) must carry the anti-forgery token
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());

    // FK/unique violations on save/delete -> friendly message instead of HTTP 500
    options.Filters.Add<DbUpdateExceptionFilter>();

    // An empty number/date field posts "" and used to show "The value '' is invalid."
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "This field is required.");

    // Decimal input must not depend on the UI language: "1.5" and "1,5" both mean 1.5 (ENG-07)
    options.ModelBinderProviders.Insert(0, new TOP4HoneyChains.Web.ModelBinding.FlexibleDecimalModelBinderProvider());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseRequestLocalization(localizationOptions);
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();