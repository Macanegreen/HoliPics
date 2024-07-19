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

namespace HoliPics.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<IdentityUser> _userManager;

        public AlbumsController(ApplicationDbContext context,
            IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;
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
                _context.Albums.Remove(album);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.Id == id);
        }

        // Get the original date and time of creation of an album
        //private DateTime GetCreationTime(int id)
        //{
        //    var album = _context.Album.FirstOrDefault(e => e.Id == id);
        //    DateTime creationTime = album.CreationTime;
        //    _context.ChangeTracker.Clear();
        //    return creationTime;
        //}

        //// Get the id and username of the creator of the album
        //private Tuple<string,string> GetCreator(int id)
        //{
        //    var album = _context.Album.FirstOrDefault(e => e.Id == id);
        //    string creatorId = album.CreatorId;
        //    string createdBy = album.CreatedBy;
        //    _context.ChangeTracker.Clear();
        //    return new Tuple<string, string>(creatorId,createdBy);
        //}

        //// Check if current user is authorized to edit or delete the album
        //private bool IsAuthorized(int id)
        //{
        //    if (GetCreator(id).Item1 != HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
        //    {   
        //        return false;
        //    }
        //    return true;
        //}
    }
}
