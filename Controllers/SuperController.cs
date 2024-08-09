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

namespace HoliPics.Controllers
{
    public abstract class SuperController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;       
        private readonly IImageService _imageService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        

        public SuperController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService, IImageService imageService, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _webHostEnvironment = webHostEnvironment;            
            _imageService = imageService;
            _userManager = userManager;
            _roleManager = roleManager;
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

        
    }
}
