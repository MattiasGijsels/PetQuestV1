// PetsAdminPanel.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // REQUIRED for IServiceScopeFactory
using Microsoft.EntityFrameworkCore; // REQUIRED for ToListAsync() and related EF Core extensions
using System.ComponentModel.DataAnnotations; // For the PetFormModel, if using it as a separate class

namespace PetQuestV1.Components.Admin
{
    public partial class PetsAdminPanelBase : ComponentBase
    {
        // Inject IServiceScopeFactory to create isolated DbContext scopes
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        // No direct injection of IPetService or UserManager here anymore

        protected List<Pet> Pets { get; set; } = new();
        protected List<Species> AvailableSpecies { get; set; } = new();
        protected List<Breed> AvailableBreeds { get; set; } = new();
        protected List<ApplicationUser> AvailableUsers { get; set; } = new();

        // Pagination pets
        protected int PetsCurrentPage { get; set; } = 1;
        protected int PetsPageSize { get; set; } = 10;
        protected int PetsTotalPages => Pets.Any() ? (int)System.Math.Ceiling((double)Pets.Count / PetsPageSize) : 1;
        protected IEnumerable<Pet> PagedPets => Pets.Skip((PetsCurrentPage - 1) * PetsPageSize).Take(PetsPageSize);

        // Forms & UI state for pets management
        // Using Pet as the form model directly. Ensure Pet class has DataAnnotations.
        protected Pet PetFormModel { get; set; } = new();
        protected bool IsPetFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;

        // Toggling section visibility for "Pets" accordion
        protected bool IsPetsSectionVisible { get; set; } = false; // Default to false (collapsed)

        protected override async Task OnInitializedAsync()
        {
            // Load initial data for the component
            await LoadData();
            await LoadDropdownData();
            // The section starts collapsed due to IsPetsSectionVisible = false
        }

        private async Task LoadData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                Pets = await petService.GetAllAsync();
            }
            PetsCurrentPage = System.Math.Clamp(PetsCurrentPage, 1, PetsTotalPages == 0 ? 1 : PetsTotalPages);
            StateHasChanged(); // Ensure UI updates after loading data
        }

        private async Task LoadDropdownData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                AvailableSpecies = await petService.GetAllSpeciesAsync();
                AvailableUsers = await userManager.Users.ToListAsync(); // Correctly using ToListAsync within scope
                AvailableBreeds = new List<Breed>(); // Ensure it's explicitly initialized for safety.
            }
            StateHasChanged(); // Ensure UI updates after loading dropdown data
        }

        // Handle Species Dropdown Change
        protected async Task OnSpeciesChanged(ChangeEventArgs e)
        {
            PetFormModel.SpeciesId = e.Value?.ToString(); // Update the SpeciesId in the model
            PetFormModel.BreedId = null; // Reset BreedId when species changes

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
                AvailableBreeds = new List<Breed>(); // Clear breeds if no species is selected
            }
            StateHasChanged(); // Force UI update after breeds are loaded
        }

        // ---------- Pets CRUD Handlers ----------
        protected void ShowAddPetForm()
        {
            PetFormModel = new Pet(); // New pet
            IsEditing = false;
            IsPetFormVisible = true;
            AvailableBreeds = new List<Breed>(); // Clear breeds for new pet form
            StateHasChanged();
        }

        protected async Task EditPet(Pet pet)
        {
            // Assign existing values for editing
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

            // Load breeds based on the existing pet's species for editing
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
            StateHasChanged(); // Force UI update after breeds are loaded for editing
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
                        // Manually map properties from form model to existing entity
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
                    // For new pets, the PetFormModel is already a new Pet object.
                    // Assuming PetFormModel has properties like Id populated (e.g. by Guid.NewGuid()) or handled by service
                    await petService.AddAsync(PetFormModel);
                }
            }

            IsPetFormVisible = false;
            await LoadData(); // Reload all data including updated pet list
            StateHasChanged(); // Ensure UI updates after form submission
        }

        protected void CancelPetForm()
        {
            IsPetFormVisible = false;
            PetFormModel = new Pet(); // Clear the form model
            StateHasChanged();
        }

        protected async Task DeletePet(string id)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                await petService.DeleteAsync(id);
            }
            await LoadData();
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
            if (!IsPetsSectionVisible) // If we are collapsing the section
            {
                IsPetFormVisible = false; // Hide the add/edit form
                PetFormModel = new Pet(); // Clear the form model
            }
            StateHasChanged();
        }
    }
}