using HoliPics.Data;
using HoliPics.Models;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elfie.Serialization;
using HoliPics.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Tokens;
using HoliPics.Areas.Identity.Data;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor;
using System.Globalization;

namespace HoliPics.Controllers
{
    public abstract class SuperController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;       
        private readonly IImageService _imageService;
        private readonly UserManager<HoliPicsUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SuperController> _logger;
        

        public SuperController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService, IImageService imageService, UserManager<HoliPicsUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<SuperController> logger)
        {
            _context = context;
            _authorizationService = authorizationService;
            _webHostEnvironment = webHostEnvironment;            
            _imageService = imageService;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> CheckPermission(Album? album, OperationAuthorizationRequirement operation)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            string returnUrl = Request.Headers.Referer.ToString();
            ViewData["returnUrl"] = returnUrl;

            // Special case where album is null during creation
            if (album == null && operation == AlbumOperations.Create)
            {                
                if (currentUserRoles.Contains("Guest"))
                {                                                         
                    return View("AccessDeniedGuest");
                }              
            }
            else
            {
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, album, operation);
                
                if (!authorizationResult.Succeeded)
                {
                    if (currentUserRoles.Contains("Guest"))
                    {
                        return View("AccessDeniedGuest");
                    }

                    return Forbid();
                }
            }
            return Ok();
        }

        public DateTime GetDateTime(Stream fileStream)
        {
            var directories = ImageMetadataReader.ReadMetadata(fileStream);
            var exifSubDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var originalDate = exifSubDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

            DateTime timeOfCreation;
            if (originalDate != null)
            {
                bool parseSucces = DateTime.TryParseExact(originalDate, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture,
                    DateTimeStyles.None, out timeOfCreation);
                if (!parseSucces) { _logger.LogInformation("DateTime {originalDate} can not be parsed.", originalDate); }
            }
            else
            {
                timeOfCreation = DateTime.Now;
                _logger.LogInformation("No DateTime metadata available for image.");
            }

            return timeOfCreation;
        }


    }
}
