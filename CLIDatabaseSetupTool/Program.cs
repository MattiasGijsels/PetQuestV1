using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PetQuestV1.Contracts.Models; // Assuming Pet, Species, Breed are here
using PetQuestV1.Data; // Assuming ApplicationDbContext, ApplicationUser are here
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CLIDatabaseSetupTool
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting PetQuestV1 CLI Tool...");

            // 1. Setup Host for Dependency Injection and Configuration
            var host = CreateHostBuilder(args).Build();

            // 2. Resolve necessary services from the service provider
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Ensure the Identity services are correctly resolved
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    Console.WriteLine("Attempting to delete and recreate database...");
                    await context.Database.EnsureDeletedAsync(); // Deletes the existing database
                    Console.WriteLine("Database deleted. Applying migrations...");
                    await context.Database.MigrateAsync();      // Applies all pending migrations, creating the schema
                    Console.WriteLine("Database migrations applied.");

                    Console.WriteLine("Seeding initial data...");
                    await SeedData(context, userManager, roleManager);
                    Console.WriteLine("Data seeding complete!");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An error occurred during database operations: {ex.Message}");
                    Console.WriteLine(ex.ToString()); // Print full stack trace for debugging
                    Console.ResetColor();
                }
            }

            Console.WriteLine("PetQuestV1 CLI Tool finished.");
        }

        // Configures the host, services, and reads configuration
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Add ApplicationDbContext
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

                    // Corrected Identity services configuration for a console app
                    services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                    {
                        options.SignIn.RequireConfirmedAccount = false; // For seeding, we don't need confirmed accounts
                        options.Password.RequireDigit = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequiredLength = 6; // Set a minimum length consistent with your new passwords
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders(); // Required for password reset/email confirmation tokens
                });

        // The core method to seed the database
        private static async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            Console.WriteLine("Seeding roles...");
            var userSeeds = ReadJsonFile<List<UserSeedData>>("users.json"); // Read user data once

            // Get all unique roles from the user seed data
            var rolesToSeed = userSeeds
                                .SelectMany(u => u.Roles) // Flatten all roles from all users
                                .Distinct()               // Get only unique role names
                                .ToList();

            foreach (var roleName in rolesToSeed)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"  Created role: {roleName}");
                }
            }
            Console.WriteLine("Roles seeded.");

            // Seed Users
            Console.WriteLine("Seeding users...");
            foreach (var userSeed in userSeeds)
            {
                var user = await userManager.FindByEmailAsync(userSeed.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = userSeed.Email,
                        Email = userSeed.Email,
                        EmailConfirmed = true, // Confirm email for easier login
                        IsDeleted = false // Ensure user is not soft-deleted
                    };
                    var result = await userManager.CreateAsync(user, userSeed.Password);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"  Created user: {user.Email}");
                        // Assign roles
                        foreach (var role in userSeed.Roles)
                        {
                            if (!await userManager.IsInRoleAsync(user, role))
                            {
                                await userManager.AddToRoleAsync(user, role);
                                Console.WriteLine($"    Assigned role '{role}' to user '{user.Email}'");
                            }
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"  Error creating user {userSeed.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            Console.WriteLine("Users seeded.");

            // Seed Species
            Console.WriteLine("Seeding species...");
            var speciesSeeds = ReadJsonFile<List<SpeciesSeedData>>("species.json");
            foreach (var speciesSeed in speciesSeeds)
            {
                if (!context.Species.Any(s => s.SpeciesName == speciesSeed.Name && !s.IsDeleted))
                {
                    context.Species.Add(new Species { SpeciesName = speciesSeed.Name });
                    Console.WriteLine($"  Added species: {speciesSeed.Name}");
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Species seeded.");

            // Seed Breeds
            Console.WriteLine("Seeding breeds...");
            var breedSeeds = ReadJsonFile<List<BreedSeedData>>("breeds.json");
            foreach (var breedSeed in breedSeeds)
            {
                if (!context.Breeds.Any(b => b.BreedName == breedSeed.Name && !b.IsDeleted))
                {
                    // Find the associated species ID
                    var species = await context.Species.FirstOrDefaultAsync(s => s.SpeciesName == breedSeed.SpeciesName && !s.IsDeleted);
                    if (species != null)
                    {
                        context.Breeds.Add(new Breed { BreedName = breedSeed.Name, SpeciesId = species.Id });
                        Console.WriteLine($"  Added breed: {breedSeed.Name} for species {breedSeed.SpeciesName}");
                    }
                    else
                    {
                        Console.Error.WriteLine($"  Warning: Species '{breedSeed.SpeciesName}' not found for breed '{breedSeed.Name}'. Skipping breed.");
                    }
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Breeds seeded.");

            // Seed Pets
            Console.WriteLine("Seeding pets...");
            var petSeeds = ReadJsonFile<List<PetSeedData>>("PetData_20250604203239.json"); // Your original pet data
            foreach (var petSeed in petSeeds)
            {
                // To avoid duplicates if the script is run multiple times without EnsureDeleted
                if (context.Pets.Any(p => p.PetName == petSeed.PetName && !p.IsDeleted))
                {
                    Console.WriteLine($"  Skipping existing pet: {petSeed.PetName}");
                    continue;
                }

                var species = await context.Species.FirstOrDefaultAsync(s => s.SpeciesName == petSeed.SpeciesName && !s.IsDeleted);
                var breed = await context.Breeds.FirstOrDefaultAsync(b => b.    BreedName == petSeed.BreedName && !b.IsDeleted);
                var owner = await userManager.FindByEmailAsync(petSeed.OwnerName); // OwnerName in your JSON is actually the Email

                if (species == null)
                {
                    Console.Error.WriteLine($"  Warning: Species '{petSeed.SpeciesName}' not found for pet '{petSeed.PetName}'. Skipping pet.");
                    continue;
                }
                if (breed == null)
                {
                    Console.Error.WriteLine($"  Warning: Breed '{petSeed.BreedName}' not found for pet '{petSeed.PetName}'. Skipping pet.");
                    continue;
                }
                // Handle N/A owner: If owner is "N/A" it's fine, otherwise if a named owner is not found, warn.
                if (owner == null && petSeed.OwnerName != "N/A")
                {
                    Console.Error.WriteLine($"  Warning: Owner '{petSeed.OwnerName}' not found for pet '{petSeed.PetName}'. Skipping pet.");
                    continue;
                }

                var pet = new Pet
                {
                    Id = Guid.TryParse(petSeed.Id, out Guid petId) ? petId.ToString() : Guid.NewGuid().ToString(), // Use existing Id or generate new
                    PetName = petSeed.PetName,
                    SpeciesId = species.Id,
                    BreedId = breed.Id,
                    OwnerId = owner?.Id, // OwnerId will be null if owner is "N/A" or not found
                    Age = petSeed.Age,
                    Advantage = petSeed.Advantage,
                    ImagePath = petSeed.ImagePath,
                    IsDeleted = false
                };
                context.Pets.Add(pet);
                Console.WriteLine($"  Added pet: {pet.PetName}");
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Pets seeded.");
        }

        // Helper to read and deserialize JSON files
        private static T ReadJsonFile<T>(string fileName)
        {
            // For a console application, files copied to output directory are typically in the same directory as the executable.
            var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found: {filePath}. Make sure it's in the project root and 'Copy to Output Directory' is set.");
            }
            var jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(jsonString) ?? throw new InvalidOperationException($"Could not deserialize {fileName}. Check JSON format.");
        }
    }

    // --- Data Transfer Objects (DTOs) for JSON Deserialization ---
    // These classes represent the structure of your JSON files
    // They should be placed in the PetQuestV1.CliTool project (e.g., directly in Program.cs or a new folder like "Models")

    public class UserSeedData
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class SpeciesSeedData
    {
        public string Name { get; set; } = default!;
    }

    public class BreedSeedData
    {
        public string Name { get; set; } = default!;
        public string SpeciesName { get; set; } = default!; // Used to link to species by name
    }

    // Matches the structure of your original PetData_20250604203239.json
    public class PetSeedData
    {
        public string Id { get; set; } = default!;
        public string PetName { get; set; } = default!;
        public string SpeciesName { get; set; } = default!; // Used to lookup SpeciesId
        public string BreedName { get; set; } = default!;   // Used to lookup BreedId
        public string OwnerName { get; set; } = default!;   // Used to lookup OwnerId (this is the email)
        public double Age { get; set; }
        public int Advantage { get; set; }
        public string? ImagePath { get; set; }
    }
}