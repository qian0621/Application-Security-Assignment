﻿using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuditLogService auditLogService;
        public IndexModel(
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager, 
            AuditLogService auditLogService) { 
            this.signInManager = signInManager; 
            this.userManager = userManager;
            this.auditLogService = auditLogService;
        }

        public ApplicationUser? CurrentUser { get; private set; } = null;

        private IDataProtector dataProtector = DataProtectionProvider.Create("EncryptData")
                .CreateProtector("MySecretKey");

        [BindProperty]
        public Login Login { get; set; } = default!;

        public class MyObject
        {
            public bool success { get; set; }
            // Add other properties if needed, based on the JSON response
        }
        /*
        public bool ValidateCaptcha()
        {
            bool result = false;



            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(
                "https://www.google.com/recaptcha/api/siteverify?secret=6LdyYWQpAAAAALLeIDlwE8Tah-__ba7sGkMnMfHs");
                using (WebResponse wResponse = req.GetResponse())
                using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                {
                    string jsonResponse = readStream.ReadToEnd();

                    // Using JavaScriptSerializer:
                    //JavaScriptSerializer js = new JavaScriptSerializer();
                    //MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);
                    //result = Convert.ToBoolean(jsonObject.success);

                    // Or, using System.Text.Json:
                    MyObject jsonObject = JsonSerializer.Deserialize<MyObject>(jsonResponse);
                    result = jsonObject.success;
                }

            return result;
        }
        */
        public async Task<IActionResult> OnGetAsync() {

            if (signInManager.IsSignedIn(User)) {
                Debug.WriteLine("User is signed in");
                CurrentUser = await userManager.FindByIdAsync(userManager.GetUserId(User));
            }
            if (CurrentUser != null) {
                CurrentUser.NRIC = dataProtector.Unprotect(CurrentUser.NRIC);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid && Login != null) {
                var identityResult = await signInManager.PasswordSignInAsync(
                    Login.Email,
                    Login.Password,
                    Login.RememberMe,
                    true    //Rate Limiting (E.g Account lockout after 3 login failures)
                );
//                if (ValidateCaptcha()) {
                    if (identityResult.Succeeded) {
                        CurrentUser = await userManager.FindByEmailAsync(Login.Email);   //User claim only updates next request
                        var sessionIDClaim = (await userManager.GetClaimsAsync(CurrentUser)).FirstOrDefault(c => c.Type == "SessionID");
                        if (sessionIDClaim == null) {
                            string sessionID = CurrentUser!.setSessionID();
                            /*await userManager.RemoveClaimsAsync(CurrentUser, await userManager.GetClaimsAsync(CurrentUser));*/
                            await userManager.AddClaimAsync(CurrentUser!, new Claim("SessionID", sessionID));
                            Debug.WriteLine("SessionID: " + sessionID);
                            // await userManager.UpdateAsync(CurrentUser!);
                            await signInManager.RefreshSignInAsync(CurrentUser!);
                            await auditLogService.LogActionAsync(CurrentUser.Id, sessionID, AuditLog.ActionType.Login);
                            return RedirectToPage();
                            //reload current page
                        } else {
                            CurrentUser = null;
                            ModelState.AddModelError("", "You are logged in somewhere else, please logout there first");
                            await signInManager.SignOutAsync();
                        }
                    } else {
                        ModelState.AddModelError("", "Username or Password incorrect");
                        if (identityResult.IsLockedOut) {
                            ModelState.AddModelError("", "Account is locked out, come back in 5 min");
                        }
                    }
                /*} else {
                    ModelState.AddModelError("", "Captcha failed");
                }*/
            }
            return Page();  //wont reload
        }
    }
}