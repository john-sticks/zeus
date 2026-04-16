using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SBInteligencia.Security
{
    public class HasPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public HasPermissionAttribute(string permission)
        {
            _permission = permission.ToUpper();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var menuService = context.HttpContext.RequestServices
                .GetRequiredService<MenuXmlService>();

            var permissions = menuService.GetPermissions(user);

            if (!permissions.Contains(_permission))
            {
                context.Result = new ForbidResult(); // 403
            }
        }
    }
}