using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HoliPics.Data;
using HoliPics.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using HoliPics.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing.Constraints;
using HoliPics.Services.Interfaces;

namespace HoliPics.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly List<string> _imageSizes;
        private readonly IImageService _imageService;

        public AlbumsController(ApplicationDbContext context,
            IAuthorizationService authorizationService, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IImageService imageService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _imageSizes = new List<string>()
            {
                "Large_", "Medium_", "Small_"
            };
            _imageService = imageService;
        }

        // GET: Albums
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Albums.ToListAsync());
        }

        // GET: Albums/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .FirstOrDefaultAsync(m => m.Id == id);
            // Check is current user is authorized to see the given album
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albums/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Albums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Album album)
        {
            if (ModelState.IsValid)
            {
                var userName = HttpContext.User.FindFirstValue(ClaimTypes.Name);
                album.CreatorId = _userManager.GetUserId(User);
                album.CreatedBy = _userManager.GetUserName(User);

                DateTime CreationTime = DateTime.Now;
                album.CreationTime = CreationTime;
                album.Thumbnail = "placeholder.png";

                _context.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(album);
        }

        // GET: Albums/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var album = await _context.Albums.FindAsync(id);

            // Check is current user is authorized to edit the given album
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Album album)
        {
            if (id != album.Id)
            {
                return NotFound();
            }           

            if (ModelState.IsValid)
            {
                try
                {   
                    var original = await _context.Albums.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

                    // Check is current user is authorized to edit the given album
                    var isAuthorized = await _authorizationService.AuthorizeAsync(User, original, AlbumOperations.Update);
                    if (!isAuthorized.Succeeded)
                    {
                        return Forbid();
                    }

                    // Assign the original unchanged properties to the new album
                    album.CreatorId = original.CreatorId;
                    album.CreatedBy = original.CreatedBy;
                    album.CreationTime = original.CreationTime;
                    album.Images = original.Images;   
                    album.Thumbnail = original.Thumbnail;

                    _context.Update(album);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.Id))
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
            return View(album);
        }

        // GET: Albums/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .FirstOrDefaultAsync(m => m.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            // Check is current user is authorized to edit the given album
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            // Check is current user is authorized to edit the given album
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (album != null)
            {
                foreach (var filename in album.Images)
                {
                    var image = await _context.Images.FirstOrDefaultAsync(im => im.FileName == filename);

                    if (image == null)
                    {
                        return BadRequest("Image does not exist.");
                    }
                    _context.Images.Remove(image);

                    await _context.SaveChangesAsync();

                    foreach (string size in _imageSizes)
                    {
                        _imageService.DeleteImageFromBlob(size + filename);
                    }
                }
                
                _context.Albums.Remove(album);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.Id == id);
        }

    }
}
