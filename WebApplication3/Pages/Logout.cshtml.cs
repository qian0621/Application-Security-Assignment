using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;

namespace WebApplication3.Pages {
    public class LogoutModel : PageModel {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuditLogService auditLogService;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuditLogService auditLogService
        ) {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.auditLogService = auditLogService;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostLogoutAsync() {
            await signInManager.SignOutAsync();

            var user = await userManager.FindByIdAsync(userManager.GetUserId(User));
            //await userManager.UpdateAsync(user);
            //remove all claims
            await userManager.RemoveClaimsAsync(user, await userManager.GetClaimsAsync(user));
            await auditLogService.LogActionAsync(user.Id, user.SessionID!, AuditLog.ActionType.Logout);
            //remove sessionID
            user.SessionID = null;
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDontLogoutAsync() {
            return RedirectToPage("Index");
        }
    }
}
