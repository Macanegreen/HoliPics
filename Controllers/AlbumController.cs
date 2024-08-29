using HoliPics.Authorization;
using HoliPics.Data;
using HoliPics.Models;
using HoliPics.VievModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using HoliPics.Areas.Identity.Data;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Globalization;
using System.Collections.Generic;


namespace HoliPics.Controllers
{
    public class AlbumController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Dictionary<string, int> _imageSizes;
        private readonly IImageService _imageService;   
        private readonly UserManager<HoliPicsUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AlbumController> _logger;

        public AlbumController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService, IImageService imageService, UserManager<HoliPicsUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AlbumController> logger)
            : base(context, webHostEnvironment, authorizationService, imageService, userManager, roleManager, logger)
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
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        // GET: Album
        [Authorize]        
        public async Task<IActionResult> Index(int id)
        {            
            var album = await _context.Albums.FindAsync(id);
            var permissionResult = await CheckPermission(album, AlbumOperations.Read);
            if (permissionResult is not OkResult) { return permissionResult; }
            ViewData["ImagesSortedByDate"] = await SortedByDate(album.Images);
            return View(album);
        }

        private async Task<SortedDictionary<DateTime, List<string>>> SortedByDate(List<string> fileNames)
        {
            SortedDictionary < DateTime, List<string> > sortedByDate = new SortedDictionary<DateTime, List<string>>();
            foreach (var fileName in fileNames)
            {
                var image = await _context.Images.FirstOrDefaultAsync(im => im.FileName == fileName);
                if (image == null) { continue; }
                // Add new (Key, Value) pair if key does not exist.
                if (!sortedByDate.TryGetValue(image.DateTaken.Date, out List<string>? imageNames))
                {
                    imageNames = new List<string>();
                    sortedByDate.Add(image.DateTaken.Date, imageNames);
                }
                imageNames.Add(fileName);
            }            
            return sortedByDate;
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
            var permissionResult = await CheckPermission(album, AlbumOperations.Create);
            if (permissionResult is not OkResult) { return permissionResult; }

            var imageView = new ImageViewModel { AlbumId = id };            
                      
            if (ModelState.IsValid)
            {                                
                long totalBytes = images.ImageFiles.Sum(f => f.Length);
                long totalReadBytes = 0;

                List<Img> imgs = new List<Img>();
                if (images.ImageFiles != null)
                {
                    foreach (var file in images.ImageFiles)
                    {
                        // Assign a unique identifier to the filename
                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);

                        Img imageFile = new Img { AlbumId = id, DateTaken = GetDateTime(file.OpenReadStream()), FileName = fileName };
                        imgs.Add(imageFile);                       

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
                album.UploadProgress = 0;
                foreach (var imageFile in imgs)
                {       
                    // Add the image filename to the album
                    album.Images.Add(imageFile.FileName);                       

                    // Track the changes to imagefile and album
                    _context.Add(imageFile);
                }
                if (album.Thumbnail == "placeholder.png")
                {
                    album.Thumbnail = "Medium_" + album.Images[0];
                }
                album.LastUpdated = DateTime.Now;

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

        [Authorize]
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
            var permissionResult = await CheckPermission(album, AlbumOperations.Read);
            if (permissionResult is not OkResult) { return permissionResult; }
          
            return View(image);
        }


        [HttpPost]
        [Authorize]        
        public async Task<IActionResult> SetThumbnail(int id)
        {        
            var image = await _context.Images.FirstOrDefaultAsync(im => im.Id == id);
            var album = await _context.Albums.FindAsync(image.AlbumId);
            var permissionResult = await CheckPermission(album, AlbumOperations.Update);
            if (permissionResult is not OkResult) { return permissionResult; }

            if (image != null)
            {
                album.Thumbnail = "Medium_" + image.FileName;
                _context.Update(album);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Picture), new {id = image.FileName});
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
            var permissionResult = await CheckPermission(album, AlbumOperations.Delete);
            if (permissionResult is not OkResult) { return permissionResult; }

            if (image == null)
            {
                return BadRequest("Image does not exist.");
            }

            album.Images.Remove(id);
            album.LastUpdated = DateTime.Now;
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
