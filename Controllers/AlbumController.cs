using HoliPics.Authorization;
using HoliPics.Data;
using HoliPics.Models;
using HoliPics.VievModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace HoliPics.Controllers
{
    public class AlbumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlbumController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _webHostEnvironment = webHostEnvironment;
        }


        // GET: Album
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {            
            var album = await _context.Albums.FindAsync(id);
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            return View(album);
        }

        public IActionResult Upload(int id)
        {   
            var imageView = new ImageViewModel { AlbumId = id };
            return View(imageView);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ImageViewModel images, int id)
        {
            var album = await _context.Albums.FindAsync(id);
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            var imageView = new ImageViewModel { AlbumId = id };
            
                      
            if (ModelState.IsValid)
            {       
                // Retrieve the uploaded images
               
                List<string> fileNames = new List<string>();
                if (images.ImageFiles != null)
                {
                    string imageFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    foreach (var file in images.ImageFiles)
                    {
                        // Assign a unique identifier to the filename
                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        fileNames.Add(fileName);

                        // Create a new file to store the uploaded image
                        string filePath = Path.Combine(imageFolder, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }

                }

                foreach (var fileName in fileNames)
                {
                    Image imageFile = new Image { AlbumId = id, FileName = fileName };

                    // Add the image filename to the album
                    album.Images.Add(imageFile.FileName);                    

                    // Track the changes to imagefile and album
                    _context.Add(imageFile);
                }
                if (album.Thumbnail == "placeholder.png")
                {
                    album.Thumbnail = album.Images[0];
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { id });                              
            }
            return View(imageView);
        }



        [Authorize]
        public async Task<IActionResult> Picture(string id) // id = FileName
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Images.FirstOrDefaultAsync(im => im.FileName == id);

            var album = await _context.Albums
                .FirstOrDefaultAsync(m => m.Id == image.AlbumId);
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

            return View(image);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetThumbnail(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Images.FirstOrDefaultAsync(im => im.FileName == id);

            var album = await _context.Albums.FindAsync(image.AlbumId);
            // Check is current user is authorized to edit the given album
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (image != null)
            {
                album.Thumbnail = image.FileName;
                _context.Update(album);
            }

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index), new { id = image.AlbumId });
        }


       


        // POST: Album/Delete-Image/5
        [HttpPost, ActionName("Delete-Image")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImageConfirmed(string id) // id = FileName
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Images.FirstOrDefaultAsync(im => im.FileName == id);

            var album = await _context.Albums.FindAsync(image.AlbumId);
            // Check is current user is authorized to edit the given album
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, album, AlbumOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (image == null)
            {
                return BadRequest("Image does not exist.");
            }

            album.Images.Remove(id);
            if (album.Thumbnail == id)
            {
                if (album.Images.Count > 0) { album.Thumbnail = album.Images[0]; }
                else { album.Thumbnail = "placeholder.png"; }
            }
            _context.Images.Remove(image);

            await _context.SaveChangesAsync();

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", image.FileName);
            System.IO.File.Delete(filePath);

            return RedirectToAction(nameof(Index), new { id=image.AlbumId });
        }

        
      

    }
}
