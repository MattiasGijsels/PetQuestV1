// PetQuestV1/Components/Admin/PetsAdminPanel.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Enums; 

namespace PetQuestV1.Components.Admin
{
    public partial class PetsAdminPanelBase : ComponentBase
    {
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        protected List<Pet> AllPets { get; set; } = new();
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
                var query = AllPets.Where(p => !p.IsDeleted).AsQueryable();

                // Apply Search Filter with null handling
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
                // The global query filter in DbContext will already exclude IsDeleted pets.
                AllPets = await petService.GetAllAsync();
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
            PetFormModel.BreedId = null; // Reset breed when species changes

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
            PetFormModel = new Pet(); // Initialize with a new Pet object
            IsEditing = false;
            IsPetFormVisible = true;
            AvailableBreeds = new List<Breed>();
            StateHasChanged();
        }

        protected async Task EditPet(Pet pet)
        {
            // Populate form model from the selected pet
            PetFormModel = new Pet
            {
                Id = pet.Id,
                PetName = pet.PetName,
                SpeciesId = pet.SpeciesId,
                OwnerId = pet.OwnerId,
                BreedId = pet.BreedId,
                Age = pet.Age,
                IsDeleted = pet.IsDeleted
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
            // Perform basic validation before proceeding
            if (string.IsNullOrWhiteSpace(PetFormModel.PetName) ||
                string.IsNullOrWhiteSpace(PetFormModel.SpeciesId) ||
                string.IsNullOrWhiteSpace(PetFormModel.OwnerId))
            {
                return;
            }

            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();

                if (IsEditing)
                {
                    await petService.UpdateAsync(PetFormModel);
                }
                else
                {
                    await petService.AddAsync(PetFormModel);
                }
            }

            IsPetFormVisible = false;
            PetFormModel = new Pet(); // Reset form model
            await LoadData(); // Reload all data after submit to reflect changes
            StateHasChanged();
        }

        protected void CancelPetForm()
        {
            IsPetFormVisible = false;
            PetFormModel = new Pet(); // Clear form
            StateHasChanged();
        }

        // --- Renamed from DeletePet to SoftDeletePet ---
        protected async Task SoftDeletePet(string id)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                await petService.SoftDeleteAsync(id);
            }
            await LoadData(); // Reload all data to reflect the soft deletion (pet will disappear due to filter)
            StateHasChanged();
        }

        // ---------- Pagination Handlers ----------
        protected void ChangePetsPage(int page)
        {
            PetsCurrentPage = System.Math.Clamp(page, 1, PetsTotalPages == 0 ? 1 : PetsTotalPages);
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
                // Toggle between Ascending and Descending using the enum
                SortDirection = (SortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                CurrentSortColumn = columnName;
                SortDirection = SortDirection.Ascending; // Default to Ascending when changing column
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

            // Use the enum for comparison
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
}