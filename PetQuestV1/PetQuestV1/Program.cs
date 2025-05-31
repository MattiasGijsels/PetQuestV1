// PetQuestV1/Program.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // Add this using directive
using PetQuestV1.Client.Pages;
using PetQuestV1.Components;
using PetQuestV1.Components.Account;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Data;
using PetQuestV1.Data.Defines;
using PetQuestV1.Data.Repository;
using PetQuestV1.Services;
using PetQuestV1.Contracts.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers(); // Add this line to enable API controllers
builder.Services.AddEndpointsApiExplorer(); // Add this line
builder.Services.AddSwaggerGen(c => // Add this block for Swagger generation
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PetQuestV1 API", Version = "v1" });
});

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
builder.Services.AddScoped<IBreedRepository, BreedRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IBreedService, BreedService>();
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
    app.UseSwagger(); // Add this line
    app.UseSwaggerUI(c => // Add this block for Swagger UI
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PetQuestV1 API V1");
    });
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

app.MapControllers(); // Add this line to map API controllers

app.Run();