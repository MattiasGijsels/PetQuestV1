using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
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

            
            var host = CreateHostBuilder(args).Build();


            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();

                if (!ValidateConnectionString(configuration))
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return; 
                }
            }

            if (!GetUserConfirmation())
            {
                Console.WriteLine("Operation cancelled by user.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return; 
            }

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    Console.WriteLine("Attempting to delete and recreate database...");
                    await context.Database.EnsureDeletedAsync(); 
                    Console.WriteLine("Database deleted. Applying migrations...");
                    await context.Database.MigrateAsync();     
                    Console.WriteLine("Database migrations applied.");

                    Console.WriteLine("Seeding initial data...");
                    await SeedData(context, userManager, roleManager);
                    Console.WriteLine("Data seeding complete!");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An error occurred during database operations: {ex.Message}");
                    Console.WriteLine(ex.ToString());
                    Console.ResetColor();
                }
            }

            Console.WriteLine("PetQuestV1 CLI Tool finished.");
        }

        private static bool GetUserConfirmation()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("!! WARNING: This tool will perform the following operations:");
            Console.WriteLine();
            Console.WriteLine("   1. DELETE the entire existing database (if it exists)");
            Console.WriteLine("   2. CREATE a new database with fresh schema");
            Console.WriteLine("   3. SEED the database with initial data");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("!! THIS WILL PERMANENTLY DELETE ALL EXISTING DATA !!");
            Console.WriteLine();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("** If you have important data, consider making a backup first **");
            Console.WriteLine();
            Console.ResetColor();

            while (true)
            {
                Console.Write("Do you want to continue? (y/n): ");
                var input = Console.ReadKey();
                Console.WriteLine(); 

                switch (input.Key)
                {
                    case ConsoleKey.Y:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✓ Proceeding with database operations...");
                        Console.ResetColor();
                        Console.WriteLine();
                        return true;

                    case ConsoleKey.N:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Operation cancelled. No changes were made to your database.");
                        Console.ResetColor();
                        return false;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Please press 'y' for Yes or 'n' for No.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private static bool ValidateConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: You have no connection string configured in appsettings.json!");
                Console.WriteLine();
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Please add a ConnectionStrings section to your appsettings.json file.");
                Console.WriteLine("Example of a correct connection string configuration:");
                Console.WriteLine();
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(@"{
  ""ConnectionStrings"": {
    ""DefaultConnection"": ""Server=(localdb)\\mssqllocaldb;Database=PetQuestV1Identity;Trusted_Connection=True;TrustServerCertificate=True;""
  }
}");
                Console.WriteLine();
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Alternative connection string examples:");
                Console.WriteLine();
                Console.WriteLine("For SQL Server Express:");
                Console.WriteLine(@"""Server=.\\SQLEXPRESS;Database=PetQuestV1Identity;Trusted_Connection=True;TrustServerCertificate=True;""");
                Console.WriteLine();
                Console.WriteLine("For SQL Server with credentials:");
                Console.WriteLine(@"""Server=localhost;Database=PetQuestV1Identity;User Id=your_username;Password=your_password;TrustServerCertificate=True;""");
                Console.WriteLine();
                Console.ResetColor();

                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Connection string found: {connectionString}");
            Console.ResetColor();
            Console.WriteLine();

            return true;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

                    services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                    {
                        options.SignIn.RequireConfirmedAccount = false; 
                        options.Password.RequireDigit = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequiredLength = 6; 
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders(); // Required for password reset/email confirmation tokens
                });

        private static async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            Console.WriteLine("Seeding roles...");
            var userSeeds = ReadJsonFile<List<UserSeedData>>("users.json"); 

            var rolesToSeed = userSeeds
                                .SelectMany(u => u.Roles) 
                                .Distinct()               
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
                        EmailConfirmed = true,
                        IsDeleted = false 
                    };
                    var result = await userManager.CreateAsync(user, userSeed.Password);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"  Created user: {user.Email}");
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


            Console.WriteLine("Seeding breeds...");
            var breedSeeds = ReadJsonFile<List<BreedSeedData>>("breeds.json");
            foreach (var breedSeed in breedSeeds)
            {

                if (!context.Breeds.Any(b => b.BreedName == breedSeed.Name && !b.IsDeleted))
                {
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

            Console.WriteLine("Seeding pets...");
            var petSeeds = ReadJsonFile<List<PetSeedData>>("PetData_20250604203239.json"); 
            foreach (var petSeed in petSeeds)
            {
                // To avoid duplicates if the script is run multiple times without EnsureDeleted
                if (context.Pets.Any(p => p.PetName == petSeed.PetName && !p.IsDeleted))
                {
                    Console.WriteLine($"  Skipping existing pet: {petSeed.PetName}");
                    continue;
                }

                var species = await context.Species.FirstOrDefaultAsync(s => s.SpeciesName == petSeed.SpeciesName && !s.IsDeleted);
                var breed = await context.Breeds.FirstOrDefaultAsync(b => b.BreedName == petSeed.BreedName && !b.IsDeleted);
                var owner = await userManager.FindByEmailAsync(petSeed.OwnerName); 

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
                if (owner == null && petSeed.OwnerName != "N/A")
                {
                    Console.Error.WriteLine($"  Warning: Owner '{petSeed.OwnerName}' not found for pet '{petSeed.PetName}'. Skipping pet.");
                    continue;
                }

                var pet = new Pet
                {
                    Id = Guid.TryParse(petSeed.Id, out Guid petId) ? petId.ToString() : Guid.NewGuid().ToString(),
                    PetName = petSeed.PetName,
                    SpeciesId = species.Id,
                    BreedId = breed.Id,
                    OwnerId = owner?.Id,
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

        private static T ReadJsonFile<T>(string fileName)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found: {filePath}. Make sure it's in the project root and 'Copy to Output Directory' is set.");
            }
            var jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(jsonString) ?? throw new InvalidOperationException($"Could not deserialize {fileName}. Check JSON format.");
        }
    }

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
        public string SpeciesName { get; set; } = default!;
    }

    public class PetSeedData
    {
        public string Id { get; set; } = default!;
        public string PetName { get; set; } = default!;
        public string SpeciesName { get; set; } = default!; 
        public string BreedName { get; set; } = default!; 
        public string OwnerName { get; set; } = default!;
        public double Age { get; set; }
        public int Advantage { get; set; }
        public string? ImagePath { get; set; }
    }
}