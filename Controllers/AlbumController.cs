using HoliPics.Authorization;
using HoliPics.Data;
using HoliPics.Models;
using HoliPics.VievModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace HoliPics.Controllers
{
    public class AlbumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Dictionary<string, int> _imageSizes;
        private readonly IImageService _imageService;        

        public AlbumController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService, IImageService imageService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _webHostEnvironment = webHostEnvironment;
            _imageSizes = new Dictionary<string, int>()
            {
                { "Large_", 1300 },
                { "Medium_",  420 },
                { "Small_", 205 }
            };
            _imageService = imageService;
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
                long totalBytes = images.ImageFiles.Sum(f => f.Length);
                long totalReadBytes = 0;

                List<string> fileNames = new List<string>();
                if (images.ImageFiles != null)
                {
                    foreach (var file in images.ImageFiles)
                    {
                        // Assign a unique identifier to the filename
                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        fileNames.Add(fileName);

                        // Record the upload progress
                        byte[] buffer = new byte[16 * 1024];
                        using (Stream stream = file.OpenReadStream())
                        {
                            int readBytes;
                            while ((readBytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalReadBytes += readBytes;
                                album.UploadProgress = (int)((float)totalReadBytes / (float)totalBytes * 100.0);
                                await _context.SaveChangesAsync();                                
                            }
                        }

                        // Store image in different sizes
                        foreach (KeyValuePair<string, int> size in _imageSizes)
                        {                          
                            using Image image = Image.Load(file.OpenReadStream());
                            image.Mutate(x => x.Resize(size.Value, 0));
                            
                            // Store the uploaded file in the cloud                            
                            using MemoryStream mutatedImageStream = new MemoryStream();                            
                            image.Save(mutatedImageStream, Image.DetectFormat(file.OpenReadStream()));
                            _imageService.UploadImageToBlob(mutatedImageStream, size.Key + fileName);
                        }
                    }

                }

                foreach (var fileName in fileNames)
                {
                    Img imageFile = new Img { AlbumId = id, FileName = fileName };

                    // Add the image filename to the album
                    album.Images.Add(imageFile.FileName);
                    album.UploadProgress = 0;

                    // Track the changes to imagefile and album
                    _context.Add(imageFile);
                }
                if (album.Thumbnail == "placeholder.png")
                {
                    album.Thumbnail = "Medium_" + album.Images[0];
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { id });                              
            }
            return View(imageView);
        }

        // Get the progress of image uploading
        [HttpGet]
        public async Task<ActionResult> Progress(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            return this.Content(album.UploadProgress.ToString());
        }

        
        public async Task<ActionResult> GetImage(string filename)
        {
            (Stream imageStream, string contentType) = await _imageService.GetImageFromBlob(filename);

            return File(imageStream, contentType);
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
                album.Thumbnail = "Medium_" + image.FileName;
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
            if (album.Thumbnail == "Medium_" + id)
            {
                if (album.Images.Count > 0) { album.Thumbnail = "Medium_" + album.Images[0]; }
                else { album.Thumbnail = "placeholder.png"; }
            }
            _context.Images.Remove(image);

            await _context.SaveChangesAsync();

            foreach (string size in _imageSizes.Keys)
            {
                _imageService.DeleteImageFromBlob(size + image.FileName);
            }

            return RedirectToAction(nameof(Index), new { id=image.AlbumId });
        }

        
      

    }
}
