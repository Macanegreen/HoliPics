using HoliPics.Data;
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
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService,
            IImageService imageService, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
            : base(context, webHostEnvironment, authorizationService, imageService, userManager, roleManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _webHostEnvironment = webHostEnvironment;
            _imageService = imageService;
            _roleManager = roleManager;
            _userManager = userManager;
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

    }
}
