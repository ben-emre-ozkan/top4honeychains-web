using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public BeekeepersController(TOP4HoneyChainsContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
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
            var tOP4HoneyChainsContext = _context.Beekeepers.Include(b => b.BeekeeperBeekeepingPurposeType).Include(b => b.BeekeeperBeekeepingType).Include(b => b.BeekeeperEducationLevel);

            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Index";

            return View(await tOP4HoneyChainsContext.ToListAsync());
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
            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle");
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle");
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle");


            ViewBag.Controller = "Beekeepers";
            ViewBag.Action = "Create";
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Beekeeper beekeeper)
        {
            if (ModelState.IsValid)
            {

                var businessNumber = await GenerateBusinessNumberAsync();


                var user = new ApplicationUser
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

                await _userManager.AddToRoleAsync(user, "Beekeeper");
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                beekeeper.BeekeeperId = user.Id;
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle", beekeeper.BeekeeperBeekeepingPurposeTypeId);
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle", beekeeper.BeekeeperBeekeepingTypeId);
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle", beekeeper.BeekeeperEducationLevelId);


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
            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle", beekeeper.BeekeeperBeekeepingPurposeTypeId);
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle", beekeeper.BeekeeperBeekeepingTypeId);
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle", beekeeper.BeekeeperEducationLevelId);


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

            ViewData["BeekeeperBeekeepingPurposeTypeId"] = new SelectList(_context.BeekeepingPurposeTypes, "BeekeepingPurposeTypeId", "BeekeepingPurposeTypeTitle", beekeeper.BeekeeperBeekeepingPurposeTypeId);
            ViewData["BeekeeperBeekeepingTypeId"] = new SelectList(_context.BeekeepingTypes, "BeekeepingTypeId", "BeekeepingTypeTitle", beekeeper.BeekeeperBeekeepingTypeId);
            ViewData["BeekeeperEducationLevelId"] = new SelectList(_context.BeekeeperEducationLevels, "BeekeeperEducationLevelId", "BeekeeperEducationLevelTitle", beekeeper.BeekeeperEducationLevelId);
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

            var user = await _userManager.FindByIdAsync(id);
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

        private bool BeekeeperExists(string id)
        {
            return _context.Beekeepers.Any(e => e.BeekeeperId == id);
        }
    }
}
