using Amazon.S3;
using Blazr.RenderState.Server;
using LuminaryVisuals.Components;
using LuminaryVisuals.Components.Account;
using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Services;
using LuminaryVisuals.Services.Events;
using LuminaryVisuals.Services.Helpers;
using LuminaryVisuals.Services.Mail;
using LuminaryVisuals.Services.Scheduled;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;
var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear(); // Trust all proxies (optional for Railway)
});
// Using Data Protection system to be saved which is needed when in production 

var keyStorageEnv = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS");
Console.WriteLine($"key is: {keyStorageEnv}");
if (!string.IsNullOrEmpty(keyStorageEnv))
{
    builder.Services.AddDataProtection()
        .SetApplicationName("YourAppName")  // optional
        .UseCustomKey(keyStorageEnv);
}
else
{
    builder.Services.AddDataProtection()
    .UseCryptographicAlgorithms(
    new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    })
    .PersistKeysToFileSystem(new DirectoryInfo(@"/app/data-protection-keys"));
}
    string? connectionString = "";
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};Port={uri.Port};SSL Mode=Require;Trust Server Certificate=true;";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

}

// Added services to the container.
builder.Services.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
        config.SnackbarConfiguration.PreventDuplicates = false;
        config.SnackbarConfiguration.NewestOnTop = false;
        config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 10000;
        config.SnackbarConfiguration.HideTransitionDuration = 500;
        config.SnackbarConfiguration.ShowTransitionDuration = 500;
        config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
    });

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IConfirmationService, ConfirmationService>();
builder.Services.AddSingleton<IMessageNotificationService, MessageNotificationService>();
// Custom Implementation of SignInManager to let new users.role Guest by default
builder.Services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();
// Our Services
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<MigratedUser>();
builder.Services.AddScoped<UserNoteService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<LoggingHours>();
builder.Services.AddScoped<SettingService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<ProjectState>();


builder.Services.AddSingleton<CircuitUpdateBroadcaster>();
builder.Services.AddHttpClient();
// Google Authentication 
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
        ?? throw new InvalidOperationException("Google ClientId not found in configuration.");
    var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
        ?? throw new InvalidOperationException("Google ClientSecret not found in configuration.");
    googleOptions.ClientId = clientId;
    googleOptions.ClientSecret = clientSecret;
    googleOptions.CallbackPath = "/signin-google";
    googleOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
    googleOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    googleOptions.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        var redirectUri = context.RedirectUri;
        redirectUri += "&prompt=select_account";
        context.Response.Redirect(redirectUri);
        return Task.CompletedTask;
    };

    googleOptions.Scope.Add("email");
    googleOptions.Scope.Add("profile");
})
.AddCookie(options =>
{
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(90);
    options.Cookie.MaxAge = TimeSpan.FromDays(90);

    // Ensure persistent cookies
    options.Cookie.Expiration = TimeSpan.FromDays(90);
    options.ExpireTimeSpan = TimeSpan.FromDays(90);
    options.SlidingExpiration = true;
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.Redirect("/");
            return Task.CompletedTask;
        },
        // Add an explicit sign-out event
        OnSigningOut = context =>
        {
            // Explicitly delete the authentication cookie
            context.CookieOptions.Expires = DateTime.UtcNow.AddDays(-1);
            context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
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

    options.User.AllowedUserNameCharacters = "abcçdefgğhijklmnoöpqrsştuüvwxyzABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-@._ ";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminClientEditor", policy =>
        policy.RequireRole("Admin", "Client", "Editor"));
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireEditorRole", policy => policy.RequireRole("Editor"));
    options.AddPolicy("RequireClientRole", policy => policy.RequireRole("Client"));
    options.AddPolicy("RequireGuestRole", policy => policy.RequireRole("Guest"));
    options.AddPolicy("AuthenticatedAccess", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddHttpContextAccessor();


builder.Services.AddSingleton<IMessageCleanupService, MessageCleanupService>();
builder.Services.AddHostedService<MessageCleanupBackgroundService>();


builder.Services.AddServerSideBlazor()
    .AddHubOptions(opt => {
        opt.MaximumReceiveMessageSize = null;
        opt.ClientTimeoutInterval = null;
        }
    )
    .AddCircuitOptions(options =>
    {
        options.DisconnectedCircuitMaxRetained = 100; // Optional: Limit the number of disconnected circuits retained.
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(120);
    });
// So now it can be unlimited
// Adds render state to control splash page
builder.AddBlazrRenderStateServerServices();
builder.Services.AddScoped<AntiforgeryStateProvider, WorkaroundEndpointAntiforgeryStateProvider>();
var environment = builder.Environment;
if (environment.IsProduction())
{
    // Get the Sentry DSN from environment variables or configuration
    var sentryDSN = Environment.GetEnvironmentVariable("SentryDSN");

    // Configure Sentry
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = sentryDSN;
        options.AutoSessionTracking = true;
        options.StackTraceMode = StackTraceMode.Enhanced;
        options.TracesSampleRate = 1.0;
        options.ProfilesSampleRate = 1.0f;
    });
}


builder.Services.Configure<CloudflareR2Settings>(configuration =>
{
    configuration.AccountId = Environment.GetEnvironmentVariable("CLOUDFLARE_ACCOUNT_ID")!;
    configuration.AccessKeyId = Environment.GetEnvironmentVariable("CLOUDFLARE_ACCESS_KEY_ID")!;
    configuration.SecretAccessKey = Environment.GetEnvironmentVariable("CLOUDFLARE_SECRET_ACCESS_KEY")!;
    configuration.publicURL = Environment.GetEnvironmentVariable("CLOUDFLARE_BUCKET_URL")!;
    configuration.BucketName = Environment.GetEnvironmentVariable("CLOUDFLARE_BUCKET_NAME")!;
});

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var r2Settings = sp.GetRequiredService<IOptions<CloudflareR2Settings>>().Value;
    AmazonS3Config config = new AmazonS3Config();
    
        config = new AmazonS3Config
        {
            ServiceURL = $"https://{r2Settings.AccountId}.r2.cloudflarestorage.com/",
            ForcePathStyle = true,
        };

    return new AmazonS3Client(
        r2Settings.AccessKeyId,
        r2Settings.SecretAccessKey,
        config
    );

});


builder.Services.AddSingleton<CloudflareR2Service>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(90);
    options.Cookie.MaxAge = TimeSpan.FromDays(90);
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<LoadingService>();

builder.Services.Configure<EmailConfiguration>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<NotificationService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<NotificationService>());

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
// Replace your existing HTTP pipeline configuration with this:
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandlingPath = "/Error", // Path to handle exceptions.
        AllowStatusCode404Response = true // allows 404 responses.
    });
    // Only use HTTPS redirection if not running in a container
        app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles(); // For now till Microsoft fix the issue with serving videos with MapStaticAssests
app.MapStaticAssets();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Initialize roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleInitializer.InitializeRoles(services);
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().WithStaticAssets();
app.MapAdditionalIdentityEndpoints();
app.Run();
