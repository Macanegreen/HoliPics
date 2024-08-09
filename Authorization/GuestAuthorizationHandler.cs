using Azure.Core;
using HoliPics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HoliPics.Authorization
{
    public class GuestAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement>
    {
        UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<GuestAuthorizationHandler> _logger;

        public GuestAuthorizationHandler(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<GuestAuthorizationHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }


        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement)
        {
            _logger.LogInformation("Evaluating authorization for guest");

            var currentUser = await _userManager.GetUserAsync(context.User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            if (currentUserRoles.Contains("Guest"))
            {
                _logger.LogInformation("Current user is a guest");
                // If asking for Read permission.
                if (requirement.Name == Constants.ReadOperationName)
                {
                    _logger.LogInformation("Guest is authorized to {operation}", requirement.Name);
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
                // Don't allow guest to Create, Update, or Delete.
                else
                {                    
                    _logger.LogInformation("Guest is NOT authorized to {operation}", requirement.Name);
                    context.Fail();
                }
            }
            return Task.CompletedTask;
        }

    }
}
