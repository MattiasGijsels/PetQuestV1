// PetQuestV1/Program.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Client.Pages;
using PetQuestV1.Components;
using PetQuestV1.Components.Account;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts;
using PetQuestV1.Data.Defines;
using PetQuestV1.Data.Repository;
using PetQuestV1.Data;
using PetQuestV1.Repositories;
using PetQuestV1.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ***** THE KEY CHANGE IS HERE *****
// 1. Register DbContextOptions as a Singleton if you want to explicitly avoid it being scoped for the factory.
//    However, AddDbContextFactory handles this internally if configured correctly.
//    The error is specific to how AddDbContextFactory's *internal* implementation might treat the options.

// Let's remove the explicit AddDbContext and just rely on the factory for all DbContext needs where appropriate.
// For Identity, it's typically AddDbContext, but we can make it work with a factory.

// Option 1 (Most common and recommended for Blazor Server/Hosted where repos use factory):
// Register DbContextOptions<TContext> as a Singleton, then use it to configure the factory.
// This ensures the options themselves are not seen as scoped when the factory is built.
builder.Services.AddSingleton<DbContextOptions<ApplicationDbContext>>(
    new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(connectionString)
        .Options);

// Then, register the DbContextFactory, and it will pick up the Singleton DbContextOptions.
builder.Services.AddDbContextFactory<ApplicationDbContext>();


// Option 2 (If you MUST keep AddDbContext for Identity, which might be fighting with factory):
// This is the more complex scenario leading to your error.
// The common fix is to register DbContextOptions as a singleton and pass it.
// If AddDbContext must remain, you might need to give it a different name or use a custom factory.
// However, let's proceed with the most direct fix for the error with the factory.
// The error is about DbContextOptions from singleton IDbContextFactory, implying the factory itself is problematic.

// Reverting to the simpler, more direct fix for the captive dependency in AddDbContextFactory:
// It's not about the connection string directly, but how AddDbContextFactory configures its *internal* options.
// The error suggests that the *internal construction* of the DbContextOptions within the factory
// is picking up something scoped.

// Let's try the *absolute simplest* AddDbContextFactory setup when the connection string is known:
// This avoids any complex lambda captures that might inadvertently refer to scoped services.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// For Identity, you need a scoped DbContext.
// If you use AddDbContextFactory for your repositories, and AddDbContext for Identity,
// sometimes EF Core can get confused.
// The simplest is to use DbContextFactory *everywhere* you need a DbContext,
// and create a scope for Identity operations if they *must* have a scoped DbContext.

// Let's try to explicitly configure DbContextOptions as Singleton for the factory path.
// This is a common workaround for this very specific error.
builder.Services.AddSingleton<DbContextOptions<ApplicationDbContext>>(sp =>
    new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(connectionString)
        .Options);

// Now AddDbContextFactory can pick up these singleton options.
builder.Services.AddDbContextFactory<ApplicationDbContext>();

// Identity still needs a Scoped DbContext. If this causes conflict, you might need to
// use a custom factory for Identity or a specific identity context separate from main one.
// Let's try keeping both for now, but ensure the AddDbContextFactory is robustly singleton-bound.
// The previous AddDbContext was:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(connectionString)); // This creates scoped DbContextOptions


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register your repositories and services.
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();

builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IUserService, UserService>();


// --- IDENTITY SETUP ---
// This part implicitly expects a Scoped ApplicationDbContext
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // This uses the AddDbContext's scoped context
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PetQuestV1.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();

app.Run();