using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HuanyuFlowerShop.Middleware
{
    public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || userRole.ToLower() != "admin")
            {
                context.Result = new JsonResult(new { 
                    Success = false, 
                    Message = "需要管理员权限才能访问此功能" 
                })
                {
                    StatusCode = 403
                };
                return;
            }
        }
    }

    public static class AdminExtensions
    {
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            return !string.IsNullOrEmpty(userRole) && userRole.ToLower() == "admin";
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}