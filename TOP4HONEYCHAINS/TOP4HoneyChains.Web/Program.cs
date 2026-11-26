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

// D-98: "create-first-admin" creates the first platform administrator of a new installation and exits (docs/KURULUM-TR.md, part B)
if (args.Length > 0 && args[0] == FirstAdminCommand.Name)
    return await FirstAdminCommand.RunAsync(args[1..], new ConsoleSecretReader());

// D-112: "create-test-set" and "remove-test-set" build and remove the live site's test set and exit (docs/KURULUM-TR.md, B9)
if (args.Length > 0 && TOP4HoneyChains.Web.Services.OrgModel.TestSet.TestSetCommand.IsCommand(args[0]))
    return await TOP4HoneyChains.Web.Services.OrgModel.TestSet.TestSetCommand.RunAsync(args, Console.Out);

var builder = WebApplication.CreateBuilder(args);
var localizationOptions = TOP4HoneyChains.Web.AppSetup.AddServices(builder);   // D-112: moved to AppSetup.cs
var app = builder.Build();
TOP4HoneyChains.Web.AppSetup.UsePipeline(app, localizationOptions);
app.Run();
return 0;
// Lets the integration tests host the app (WebApplicationFactory<Program>)
public partial class Program { }
