using LuminaryVisuals.Components;
using LuminaryVisuals.Components.Account;
using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MudBlazor;

var builder = WebApplication.CreateBuilder(args);

// Added services to the container.
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
});
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Custom Implementation of SignInManager to let new users.role Guest by default
builder.Services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();
// Our Services
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<UserNoteService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SettingService>();
builder.Services.AddHttpClient();
// Google Authentication 
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    var clientId = builder.Configuration["Authentication:Google:ClientId"]
        ?? throw new InvalidOperationException("Google ClientId not found in configuration.");
    var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
        ?? throw new InvalidOperationException("Google ClientSecret not found in configuration.");

    googleOptions.ClientId = clientId;
    googleOptions.ClientSecret = clientSecret;
    googleOptions.CallbackPath = "/signin-google";

    googleOptions.Scope.Add("email");
    googleOptions.Scope.Add("profile");
})
.AddCookie(options =>
{
    options.LoginPath = "/";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.SlidingExpiration = true; 
});

// database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Disable password requirements since we're using Google authentication only
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 0;

    // Require unique email as this will be our identifier from Google
    options.User.RequireUniqueEmail = true;

    // No need for email confirmation as Google accounts are already verified
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure security headers and cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

// Configure email sender (not currently used for anything, just leaving it incase we need it)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireEditorRole", policy => policy.RequireRole("Editor"));
    options.AddPolicy("RequireClientRole", policy => policy.RequireRole("Client"));
    options.AddPolicy("RequireGuestRole", policy => policy.RequireRole("Guest"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/AccessDenied";
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.Redirect("/accessdenied");
            return Task.CompletedTask;
        }
    };
});


// IMPORTANT THIS BELOW REMOVE THE LIMIT OF 16k character of SignalR on how big a message can be.
builder.Services.AddServerSideBlazor().AddHubOptions(opt => opt.MaximumReceiveMessageSize = null);
// So now it can be unlimited


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAntiforgery();

app.UseAuthorization();

// Initialize roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleInitializer.InitializeRoles(services);
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
