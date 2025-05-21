// PetsAdminPanel.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using System.Collections.Generic;
using System.Linq; // Make sure this is present for LINQ methods
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore; // Needed for ToListAsync() and related EF Core extensions
using System.ComponentModel.DataAnnotations; // For the PetFormModel, if using it as a separate class

namespace PetQuestV1.Components.Admin
{
    public partial class PetsAdminPanelBase : ComponentBase
    {
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        protected List<Pet> AllPets { get; set; } = new(); // Store all pets initially
        protected List<Species> AvailableSpecies { get; set; } = new();
        protected List<Breed> AvailableBreeds { get; set; } = new();
        protected List<ApplicationUser> AvailableUsers { get; set; } = new();

        // --- Sorting Properties ---
        protected string CurrentSortColumn { get; set; } = "PetName"; // Default sort column
        protected SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        // --- Search Property ---
        protected string SearchTerm { get; set; } = string.Empty;

        // Pagination properties
        protected int PetsCurrentPage { get; set; } = 1;
        protected int PetsPageSize { get; set; } = 10;

        // Computed property for filtered and sorted pets
        protected IEnumerable<Pet> FilteredAndSortedPets
        {
            get
            {
                var query = AllPets.AsQueryable();

                // Apply Search Filter with robust null handling
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(p =>
                        p.PetName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                        (p.Species != null && !string.IsNullOrEmpty(p.Species.SpeciesName) && p.Species.SpeciesName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase)) ||
                        (p.Breed != null && !string.IsNullOrEmpty(p.Breed.BreedName) && p.Breed.BreedName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase)) ||
                        (p.Owner != null && !string.IsNullOrEmpty(p.Owner.UserName) && p.Owner.UserName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase))
                    );
                }

                // Apply Sorting with explicit null handling for OrderBy/OrderByDescending
                // This directly addresses the CS8602 warnings.
                query = CurrentSortColumn switch
                {
                    "PetName" => SortDirection == SortDirection.Ascending ? query.OrderBy(p => p.PetName) : query.OrderByDescending(p => p.PetName),
                    "Species" => SortDirection == SortDirection.Ascending ?
                                   query.OrderBy(p => p.Species == null ? string.Empty : p.Species.SpeciesName).ThenBy(p => p.PetName) :
                                   query.OrderByDescending(p => p.Species == null ? string.Empty : p.Species.SpeciesName).ThenByDescending(p => p.PetName),
                    "Breed" => SortDirection == SortDirection.Ascending ?
                                 query.OrderBy(p => p.Breed == null ? string.Empty : p.Breed.BreedName) :
                                 query.OrderByDescending(p => p.Breed == null ? string.Empty : p.Breed.BreedName),
                    "Age" => SortDirection == SortDirection.Ascending ? query.OrderBy(p => p.Age) : query.OrderByDescending(p => p.Age),
                    "Owner" => SortDirection == SortDirection.Ascending ?
                                 query.OrderBy(p => p.Owner == null ? string.Empty : p.Owner.UserName) :
                                 query.OrderByDescending(p => p.Owner == null ? string.Empty : p.Owner.UserName),
                    _ => query.OrderBy(p => p.PetName) // Default sort
                };

                return query.ToList(); // Materialize the filtered and sorted list
            }
        }

        protected int PetsTotalPages => FilteredAndSortedPets.Any() ? (int)System.Math.Ceiling((double)FilteredAndSortedPets.Count() / PetsPageSize) : 1;
        protected IEnumerable<Pet> PagedPets => FilteredAndSortedPets.Skip((PetsCurrentPage - 1) * PetsPageSize).Take(PetsPageSize);


        // Forms & UI state for pets management
        protected Pet PetFormModel { get; set; } = new();
        protected bool IsPetFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;

        // Toggling section visibility for "Pets" accordion
        protected bool IsPetsSectionVisible { get; set; } = false; // Default to false (collapsed)

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await LoadDropdownData();
            // Ensure pagination is correct after initial load
            PetsCurrentPage = System.Math.Clamp(PetsCurrentPage, 1, PetsTotalPages == 0 ? 1 : PetsTotalPages);
        }

        private async Task LoadData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                AllPets = await petService.GetAllAsync(); // Load all pets
            }
            StateHasChanged(); // Refresh UI after loading data
        }

        private async Task LoadDropdownData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                AvailableSpecies = await petService.GetAllSpeciesAsync();
                AvailableUsers = await userManager.Users.ToListAsync();
                AvailableBreeds = new List<Breed>(); // Ensure it's explicitly initialized for safety.
            }
            StateHasChanged();
        }

        protected async Task OnSpeciesChanged(ChangeEventArgs e)
        {
            PetFormModel.SpeciesId = e.Value?.ToString();
            PetFormModel.BreedId = null;

            if (!string.IsNullOrEmpty(PetFormModel.SpeciesId))
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                    AvailableBreeds = await petService.GetBreedsBySpeciesIdAsync(PetFormModel.SpeciesId);
                }
            }
            else
            {
                AvailableBreeds = new List<Breed>();
            }
            StateHasChanged();
        }

        protected void ShowAddPetForm()
        {
            PetFormModel = new Pet();
            IsEditing = false;
            IsPetFormVisible = true;
            AvailableBreeds = new List<Breed>();
            StateHasChanged();
        }

        protected async Task EditPet(Pet pet)
        {
            PetFormModel = new Pet
            {
                Id = pet.Id,
                PetName = pet.PetName,
                SpeciesId = pet.Species?.Id,
                OwnerId = pet.Owner?.Id,
                BreedId = pet.Breed?.Id,
                Age = pet.Age
            };
            IsEditing = true;
            IsPetFormVisible = true;

            if (!string.IsNullOrEmpty(PetFormModel.SpeciesId))
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                    AvailableBreeds = await petService.GetBreedsBySpeciesIdAsync(PetFormModel.SpeciesId);
                }
            }
            else
            {
                AvailableBreeds = new List<Breed>();
            }
            StateHasChanged();
        }

        protected async Task HandlePetFormSubmit()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();

                if (IsEditing)
                {
                    var existingPet = await petService.GetByIdAsync(PetFormModel.Id);
                    if (existingPet != null)
                    {
                        existingPet.PetName = PetFormModel.PetName;
                        existingPet.Age = PetFormModel.Age;
                        existingPet.SpeciesId = PetFormModel.SpeciesId;
                        existingPet.BreedId = PetFormModel.BreedId;
                        existingPet.OwnerId = PetFormModel.OwnerId;

                        await petService.UpdateAsync(existingPet);
                    }
                }
                else
                {
                    await petService.AddAsync(PetFormModel);
                }
            }

            IsPetFormVisible = false;
            await LoadData(); // Reload all data after submit
            StateHasChanged();
        }

        protected void CancelPetForm()
        {
            IsPetFormVisible = false;
            PetFormModel = new Pet();
            StateHasChanged();
        }

        protected async Task DeletePet(string id)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                await petService.DeleteAsync(id);
            }
            await LoadData(); // Reload all data after deletion
            StateHasChanged();
        }

        // ---------- Pagination Handlers ----------
        protected void ChangePetsPage(int page)
        {
            PetsCurrentPage = page < 1 ? 1 : page > PetsTotalPages ? PetsTotalPages : page;
            StateHasChanged();
        }

        // ---------- Section Visibility Toggle Methods ----------
        protected void TogglePetsSection()
        {
            IsPetsSectionVisible = !IsPetsSectionVisible;
            if (!IsPetsSectionVisible)
            {
                IsPetFormVisible = false;
                PetFormModel = new Pet();
            }
            StateHasChanged();
        }

        // --- Sorting Methods ---
        protected void SortBy(string columnName)
        {
            if (CurrentSortColumn == columnName)
            {
                // Toggle direction if same column
                SortDirection = (SortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                // New column, default to ascending
                CurrentSortColumn = columnName;
                SortDirection = SortDirection.Ascending;
            }
            PetsCurrentPage = 1; // Reset to first page on sort
            StateHasChanged(); // Re-render to apply sort
        }

        protected string GetSortIcon(string columnName)
        {
            if (CurrentSortColumn != columnName)
            {
                return "bi-sort-alpha-down"; // Default neutral sort icon
            }

            return SortDirection == SortDirection.Ascending ? "bi-sort-alpha-down" : "bi-sort-alpha-up";
        }

        // --- Search Handler ---
        protected void OnSearchInput(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? string.Empty;
            PetsCurrentPage = 1; // Reset to first page on search
            StateHasChanged(); // Re-render to apply filter
        }
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}