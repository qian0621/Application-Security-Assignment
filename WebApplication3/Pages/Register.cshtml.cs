using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class RegisterModel : PageModel
    {
        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }
        private AuditLogService auditLogService { get; }

        public RegisterModel(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            AuditLogService auditLogService
        ) {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.auditLogService = auditLogService;
        }

        public string passwordRegex { get; } = Register.passwordRegex;

        private int maxSize = 1024 * 1024;

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Register Register { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || Register == null)
            {
                return Page();
            }

            var dataProtector = DataProtectionProvider.Create("EncryptData")
                .CreateProtector("MySecretKey");
            var user = new ApplicationUser()
            {
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
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                Console.WriteLine(error.Description);
            }
            return Page();
        }
    }
}
