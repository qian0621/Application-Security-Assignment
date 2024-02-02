using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;

namespace WebApplication3.Pages
{
    public class LogoutModel : PageModel
    {
        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }
        private AuditLogService auditLogService { get; }

        public LogoutModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AuditLogService auditLogService
        ) {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.auditLogService = auditLogService;
        }
        public void OnGet() { }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await signInManager.SignOutAsync();

            var user = await userManager.FindByIdAsync(userManager.GetUserId(User));
            //remove sessionID
            user.SessionID = null;
            //await userManager.UpdateAsync(user);
            //remove all claims
            await userManager.RemoveClaimsAsync(user, await userManager.GetClaimsAsync(user));
            await auditLogService.LogActionAsync(user.Id, user.SessionID!, AuditLog.ActionType.Logout);
            return RedirectToPage("Index");
        }
        public async Task<IActionResult> OnPostDontLogoutAsync()
        {
            return RedirectToPage("Index");
        }
    }
}
