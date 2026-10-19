using TOP4HoneyChains.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class BeekeepersController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TOP4HoneyChains.Web.Services.OrgModel.FeatureFlags _flags;
        public BeekeepersController(TOP4HoneyChainsContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            Microsoft.Extensions.Options.IOptions<TOP4HoneyChains.Web.Services.OrgModel.FeatureFlags> flags)
        {
            _flags = flags.Value;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        private async Task<string> GenerateBusinessNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var countryCode = "TR";

            var countThisYear = await _context.Beekeepers
                .CountAsync(b => b.BeekeeperBusinessNumber.Contains($"BEE-{year}-"));

            var nextNumber = countThisYear + 1;

            return $"{countryCode}-BEE-{year}-{nextNumber.ToString("D6")}";
        }




        public async Task<IActionResult> Index()
        {
            // D-73: the list never loads the photos; it shows a small cached thumbnail (Thumb) or the initials
            var beekeepers = await _context.Beekeepers.AsNoTracking().Select(b => new Beekeeper
            {
                BeekeeperId = b.BeekeeperId, BeekeeperFirstName = b.BeekeeperFirstName, BeekeeperLastName = b.BeekeeperLastName,
                BeekeeperBirthDate = b.BeekeeperBirthDate, BeekeeperExperienceTime = b.BeekeeperExperienceTime,
                BeekeeperPhoneNumber = b.BeekeeperPhoneNumber, BeekeeperBusinessNumber = b.BeekeeperBusinessNumber,
                BeekeeperEducationLevelId = b.BeekeeperEducationLevelId, BeekeeperBeekeepingPurposeTypeId = b.BeekeeperBeekeepingPurposeTypeId,
                BeekeeperBeekeepingTypeId = b.BeekeeperBeekeepingTypeId, IsActive = b.IsActive, DeactivatedAt = b.DeactivatedAt,
                BeekeeperBeekeepingPurposeType = b.BeekeeperBeekeepingPurposeType, BeekeeperBeekeepingType = b.BeekeeperBeekeepingType,
                BeekeeperEducationLevel = b.BeekeeperEducationLevel
            }).ToListAsync();
            // photo size per beekeeper: tells which ones have a photo and versions the thumbnail URL (a new photo, a new URL)
            ViewBag.PhotoVersions = await _context.Beekeepers.AsNoTracking().Where(b => b.BeekeeperProfilePhoto != null)
                .Select(b => new { b.BeekeeperId, Size = EF.Functions.DataLength(b.BeekeeperProfilePhoto) })
                .ToDictionaryAsync(b => b.BeekeeperId!, b => b.Size ?? 0);

            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Index";

            return View(beekeepers);
        }

        // D-73: a 96 px thumbnail of the profile photo, cached on the server and in the browser (private: personal data)
        [HttpGet]
        public async Task<IActionResult> Thumb(string id, int v, [FromServices] Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            var key = $"bk-thumb:{id}:{v}";
            if (!cache.TryGetValue(key, out byte[]? thumb))
            {
                var photo = await _context.Beekeepers.AsNoTracking().Where(b => b.BeekeeperId == id).Select(b => b.BeekeeperProfilePhoto).FirstOrDefaultAsync();
                if (photo == null || photo.Length == 0)
                    return NotFound();
                try
                {
                    using var image = SixLabors.ImageSharp.Image.Load(photo);
                    image.Mutate(x => x.AutoOrient().Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(96, 96),
                        Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop
                    }));
                    using var output = new MemoryStream();
                    await image.SaveAsJpegAsync(output, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 80 });
                    thumb = output.ToArray();
                }
                catch (SixLabors.ImageSharp.ImageFormatException)
                {
                    return NotFound();
                }
                cache.Set(key, thumb, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(6), Size = thumb.Length });
            }
            Response.Headers.CacheControl = "private, max-age=86400";
            return File(thumb!, "image/jpeg");
        }


        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeeper = await _context.Beekeepers
                .Include(b => b.BeekeeperBeekeepingPurposeType)
                .Include(b => b.BeekeeperBeekeepingType)
                .Include(b => b.BeekeeperEducationLevel)
                .FirstOrDefaultAsync(m => m.BeekeeperId == id);
            if (beekeeper == null)
            {
                return NotFound();
            }



            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Details";

            return View(beekeeper);
        }


        public IActionResult Create()
        {
            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle").Translated(HttpContext, "BeekeepingPurposeType", "Title");
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle").Translated(HttpContext, "BeekeepingType", "Title");
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle").Translated(HttpContext, "EducationLevel", "Title");


            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Create";
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Beekeeper beekeeper)
        {
            ApplyPhone(beekeeper, stored: null);
            await ValidateProfilePhotoAsync(beekeeper);

            if (ModelState.IsValid)
            {

                var businessNumber = await GenerateBusinessNumberAsync();

                // With the organization model a beekeeper is a record without an account (D-23); the
                // association records data for them. v1.0 keeps creating a locked login.
                ApplicationUser? user = null;
                if (!_flags.OrgRoleModel)
                {
                    user = new ApplicationUser
                    {
                        UserName = businessNumber, // veya email alanı
                        Email = businessNumber + "@top4honeychains.com" // geçici
                    };

                    // Beekeepers do not log in (D-01, D-09): the account gets a random, never-shown
                    // password (the business number used to be the password, which is guessable)
                    var result = await _userManager.CreateAsync(user, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError("", error.Description);

                        return View(beekeeper);
                    }
                }

                // The login (Identity DB context) and the Beekeepers row (business DB context) are saved
                // separately. If anything after this point fails, remove the login again so a retry does not
                // fail with "user name taken" (ENG-01).
                try
                {
                    if (user != null)
                    {
                        await _userManager.AddToRoleAsync(user, "Beekeeper");
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        beekeeper.BeekeeperId = user.Id;
                    }
                    else
                    {
                        beekeeper.BeekeeperId = Guid.NewGuid().ToString();
                    }
                    beekeeper.BeekeeperBusinessNumber = businessNumber;

                    if (beekeeper.ProfilePhotoFile != null && beekeeper.ProfilePhotoFile.Length > 0)
                    {
                        using var image = await Image.LoadAsync(beekeeper.ProfilePhotoFile.OpenReadStream());

                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(300, 300),
                            Mode = ResizeMode.Crop
                        }));

                        using var memoryStream = new MemoryStream();

                        await image.SaveAsync(memoryStream, new JpegEncoder
                        {
                            Quality = 80
                        });

                        beekeeper.BeekeeperProfilePhoto = memoryStream.ToArray();
                    }

                    _context.Add(beekeeper);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    if (user != null)
                        await _userManager.DeleteAsync(user);
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle", beekeeper.BeekeeperBeekeepingPurposeTypeId).Translated(HttpContext, "BeekeepingPurposeType", "Title");
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle", beekeeper.BeekeeperBeekeepingTypeId).Translated(HttpContext, "BeekeepingType", "Title");
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle", beekeeper.BeekeeperEducationLevelId).Translated(HttpContext, "EducationLevel", "Title");


            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Create";
            return View(beekeeper);
        }

        // GET: Beekeepers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beekeeper = await _context.Beekeepers.FindAsync(id);
            if (beekeeper == null)
            {
                return NotFound();
            }
            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle", beekeeper.BeekeeperBeekeepingPurposeTypeId).Translated(HttpContext, "BeekeepingPurposeType", "Title");
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle", beekeeper.BeekeeperBeekeepingTypeId).Translated(HttpContext, "BeekeepingType", "Title");
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle", beekeeper.BeekeeperEducationLevelId).Translated(HttpContext, "EducationLevel", "Title");


            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Edit";
            return View(beekeeper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Beekeeper beekeeper)
        {
            if (id != beekeeper.BeekeeperId)
            {
                return NotFound();
            }

            var existingBeekeeper = await _context.Beekeepers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BeekeeperId == id);

            if (existingBeekeeper == null)
            {
                return NotFound();
            }

            // Status only changes through Deactivate/Activate (D-04).
            // Copied before validation so a failed POST re-renders the real status.
            beekeeper.IsActive = existingBeekeeper.IsActive;
            beekeeper.DeactivatedAt = existingBeekeeper.DeactivatedAt;

            ApplyPhone(beekeeper, existingBeekeeper.BeekeeperPhoneNumber);
            await ValidateProfilePhotoAsync(beekeeper);

            if (ModelState.IsValid)
            {
                try
                {
                    beekeeper.BeekeeperBusinessNumber = existingBeekeeper.BeekeeperBusinessNumber;

                    if (beekeeper.ProfilePhotoFile != null && beekeeper.ProfilePhotoFile.Length > 0)
                    {
                        using var image = await SixLabors.ImageSharp.Image.LoadAsync(beekeeper.ProfilePhotoFile.OpenReadStream());

                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new SixLabors.ImageSharp.Size(300, 300),
                            Mode = ResizeMode.Crop
                        }));

                        using var memoryStream = new MemoryStream();
                        await image.SaveAsJpegAsync(memoryStream);

                        beekeeper.BeekeeperProfilePhoto = memoryStream.ToArray();
                    }
                    else
                    {
                        // Eski foto korunur
                        beekeeper.BeekeeperProfilePhoto = existingBeekeeper.BeekeeperProfilePhoto;
                    }

                    _context.Update(beekeeper);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BeekeeperExists(beekeeper.BeekeeperId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }




                return RedirectToAction(nameof(Index));
            }

            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle", beekeeper.BeekeeperBeekeepingPurposeTypeId).Translated(HttpContext, "BeekeepingPurposeType", "Title");
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle", beekeeper.BeekeeperBeekeepingTypeId).Translated(HttpContext, "BeekeepingType", "Title");
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle", beekeeper.BeekeeperEducationLevelId).Translated(HttpContext, "EducationLevel", "Title");
            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Edit";
            return View(beekeeper);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var beekeeper = await _context.Beekeepers.FindAsync(id);
            if (beekeeper == null)
                return NotFound();

            beekeeper.IsActive = false;
            beekeeper.DeactivatedAt = DateTime.UtcNow;

            // With the organization model, beekeeper logins are left untouched: they stay locked (D-09, D-23)
            var user = _flags.OrgRoleModel ? null : await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var beekeeper = await _context.Beekeepers.FindAsync(id);
            if (beekeeper == null)
                return NotFound();

            beekeeper.IsActive = true;
            beekeeper.DeactivatedAt = null;

            // The login stays locked: beekeepers do not log in in this release (D-01, D-09)

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // D-78: a new or changed phone number is validated and stored in the international form (+905321234567);
        // an unchanged stored number (also one saved before D-78) is kept exactly as it is.
        private void ApplyPhone(Beekeeper beekeeper, string? stored)
        {
            var input = beekeeper.BeekeeperPhoneNumber?.Trim();
            if (stored != null && input == stored.Trim())
            {
                beekeeper.BeekeeperPhoneNumber = stored;
                return;
            }
            beekeeper.BeekeeperPhoneNumber = Services.PhoneNumberPolicy.Normalize(input, out var error);
            if (error != null)
            {
                beekeeper.BeekeeperPhoneNumber = input;
                ModelState.AddModelError(nameof(Beekeeper.BeekeeperPhoneNumber), error);
            }
        }

        private async Task ValidateProfilePhotoAsync(Beekeeper beekeeper)
        {
            if (beekeeper.ProfilePhotoFile == null || beekeeper.ProfilePhotoFile.Length == 0)
                return;

            var error = await UploadValidator.ValidateImageAsync(beekeeper.ProfilePhotoFile);
            if (error != null)
                ModelState.AddModelError(nameof(Beekeeper.ProfilePhotoFile), error);
        }

        private bool BeekeeperExists(string id)
        {
            return _context.Beekeepers.Any(e => e.BeekeeperId == id);
        }
    }
}
