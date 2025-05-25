// PetQuestV1/Program.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Client.Pages;
using PetQuestV1.Components;
using PetQuestV1.Components.Account;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Data;
using PetQuestV1.Data.Defines; // For ISpeciesRepository, IPetRepository
using PetQuestV1.Data.Repository; // For SpeciesRepository, PetRepository
using PetQuestV1.Services;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts;
using PetQuestV1.Repositories; // Ensure ApplicationUser is accessible

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

// --- SINGLE DB CONTEXT REGISTRATION: Use AddDbContextFactory for everything ---
// This registers IDbContextFactory<ApplicationDbContext> as a Singleton.
// Its options will be configured here.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register your repositories and services. They will now use the IDbContextFactory.
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();

builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IUserService, UserService>();


// --- IDENTITY SETUP ---
// This ensures ApplicationDbContext is registered as Scoped, using the factory, for Identity's needs.
builder.Services.AddScoped<ApplicationDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

// This is the SINGLE and COMPLETE IdentityCore registration.
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // This correctly picks up the Scoped ApplicationDbContext
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

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();