using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages {
    public class RegisterModel : PageModel {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuditLogService auditLogService;

        public RegisterModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, 
            AuditLogService auditLogService
        ) {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.auditLogService = auditLogService;
        }

        private IDataProtector dataProtector = DataProtectionProvider.Create("EncryptData")
                .CreateProtector("MySecretKey");

        public string passwordRegex { get; } = Register.passwordRegex;

        [BindProperty]
        public Register Register { get; set; } = default!;

        private int maxSize = 1024 * 1024;

        public IActionResult OnGet() => Page();
        
        public async Task<IActionResult> OnPostAsync() {
          if (!ModelState.IsValid || Register == null) { return Page(); }
            var user = new ApplicationUser() {
                FirstName = Register.FirstName,
                LastName = Register.LastName,
                Gender = Register.Gender,
                NRIC = dataProtector.Protect(Register.NRIC),  //(Must be encrypted)
                UserName = Register.Email,  //defaults to empty string which is not allowed
                Email = Register.Email,
                BirthDate = Register.BirthDate,
                Resume = Path.GetRandomFileName() + Path.GetExtension(Register.Resume.FileName),
                WhoAmI = Register.WhoAmI
            };
            var result = await userManager.CreateAsync(user, Register.Password);
            if (result.Succeeded) {
                await auditLogService.LogActionAsync(user.Id, user.SessionID!, AuditLog.ActionType.Register);
                using (var stream = System.IO.File.Create(Path.Combine("wwwroot", "uploads", user.Resume))) {
                    await Register.Resume.CopyToAsync(stream);
                }
                await userManager.AddClaimAsync(user, new Claim("SessionID", user.SessionID!));
                await signInManager.SignInAsync(user, false);
                await auditLogService.LogActionAsync(user.Id, user.SessionID!, AuditLog.ActionType.Login);
                return RedirectToPage("Index");
            }
            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
                Console.WriteLine(error.Description);
            }
            return Page();
        }
    }
}
