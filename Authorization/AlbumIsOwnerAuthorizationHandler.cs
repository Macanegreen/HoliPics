using Azure.Core;
using HoliPics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HoliPics.Authorization
{
    public class AlbumIsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Album>
    {
        UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AlbumIsOwnerAuthorizationHandler> _logger;

        public AlbumIsOwnerAuthorizationHandler(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AlbumIsOwnerAuthorizationHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }


        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   Album resource)
        {
            _logger.LogInformation("Evaluating authorization for ownership of album");


            if (context.User == null || resource == null)
            {
                _logger.LogInformation("Album or user is null");
                return Task.CompletedTask;
            }

            // If not asking for CRUD permission, return.
            if (requirement.Name != Constants.CreateOperationName &&
                requirement.Name != Constants.ReadOperationName &&
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (resource.CreatorId == _userManager.GetUserId(context.User))
            {
                _logger.LogInformation("Current user is owner of requested album");                                
                context.Succeed(requirement);
            }
            // Don't allow users to access albums they do not own.
            else
            {
                _logger.LogInformation("Current user is NOT the owner of requested album");
                context.Fail();
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

    }
}
