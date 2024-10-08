﻿using HoliPics.Areas.Identity.Data;
using HoliPics.Data;
using HoliPics.Models;
using HoliPics.Services.Implementations;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoliPics.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<HoliPicsUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService,
            IImageService imageService, RoleManager<IdentityRole> roleManager, UserManager<HoliPicsUser> userManager, ILogger<AdminController> logger)
            : base(context, webHostEnvironment, authorizationService, imageService, userManager, roleManager, logger)
        {
            _context = context;
            _authorizationService = authorizationService;
            _webHostEnvironment = webHostEnvironment;
            _imageService = imageService;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Overview()
        {
            ViewData["Users"] = await _userManager.Users.ToListAsync();
            ViewData["Roles"] = await _roleManager.Roles.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {            
            Console.WriteLine(roleName);
            var role = new IdentityRole { Name = roleName };
            await _roleManager.CreateAsync(role);
            return RedirectToAction(nameof(Overview));
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {           
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.AddToRoleAsync(user, roleName);
            return RedirectToAction(nameof(Overview));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {            
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.RemoveFromRoleAsync(user, roleName);
            return RedirectToAction(nameof(Overview));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromServices] IAlbumDeleteService albumDeleteService, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            foreach (int albumId in user.Albums)
            {
                Console.WriteLine(albumId.ToString());
                await albumDeleteService.DeleteAlbum(albumId);
            }

            var result = await _userManager.DeleteAsync(user);
           
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }
            return RedirectToAction(nameof(Overview));
        }

        
        public async Task<IActionResult> UpdateImagesWithDateTime()
        {
            List<Img> imgs = await _context.Images.ToListAsync();
            foreach (Img img in imgs)
            {
                if (img.DateTaken == DateTime.MinValue)
                {
                    (Stream imgStream, string contentType) = await _imageService.GetImageFromBlob("Small_" + img.FileName);
                    var dateTaken = GetDateTime(imgStream);
                    img.DateTaken = dateTaken;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Overview));
        }



    }
}
