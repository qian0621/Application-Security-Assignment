using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using WebApplication3.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//save user data in database
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Lockout = new LockoutOptions() {
        DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5),   //Automatic account recovery after x mins of lockout.
        MaxFailedAccessAttempts = 3 //Rate Limiting (E.g Account lockout after 3 login failures)
    };
})
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options => {    //Set Strong password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    //above are default settings
    options.Password.RequiredLength = 12;
    options.User.RequireUniqueEmail = true;
});


//IDataProtector
builder.Services.AddDataProtection();

//Audit Logging
builder.Services.AddScoped<AuditLogService>();

//Recaptcha
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));


builder.Services.ConfigureApplicationCookie(options => {
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // session timeout
    options.SlidingExpiration = true; // Reset expiration timer when active
    options.Cookie.HttpOnly = true; // limit access from client-side scripts
    options.LoginPath = "/";    //Route to homepage/login page after session timeout
    options.AccessDeniedPath = "/";
    options.Events.OnValidatePrincipal = async context => {
        HttpContext httpContext = context.HttpContext;

        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var principal = context.Principal;
        var user = await userManager.FindByIdAsync(userManager.GetUserId(principal));
        var contextSessionID = principal?.FindFirst("SessionID")?.Value;
        var userSessionID = (await userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "SessionID")?.Value;
        Debug.WriteLine("SessionIDClaim: \n" + (contextSessionID ?? "null") + "\n" + (userSessionID ?? "null"));
        if (contextSessionID == null || userSessionID == null || userSessionID != contextSessionID) {
            // The session ID is different, so logged in elsewhere
            context.RejectPrincipal();
            await httpContext.SignOutAsync();
            return;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/errors");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
