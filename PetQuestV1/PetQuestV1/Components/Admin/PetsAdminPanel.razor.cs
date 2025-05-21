using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Components.Admin
{
    public partial class PetsAdminPanelBase : ComponentBase
    {
        [Inject] public IPetService PetService { get; set; } = default!;
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; } = default!;

        protected List<Pet> Pets { get; set; } = new();
        protected List<Species> AvailableSpecies { get; set; } = new();
        protected List<Breed> AvailableBreeds { get; set; } = new();
        protected List<ApplicationUser> AvailableUsers { get; set; } = new();

        // Pagination pets
        protected int PetsCurrentPage { get; set; } = 1;
        protected int PetsPageSize { get; set; } = 10;
        protected int PetsTotalPages => (int)System.Math.Ceiling((double)Pets.Count / PetsPageSize);
        protected IEnumerable<Pet> PagedPets => Pets.Skip((PetsCurrentPage - 1) * PetsPageSize).Take(PetsPageSize);

        // Forms & UI state for pets management
        protected Pet PetFormModel { get; set; } = new();
        protected bool IsPetFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;

        // Toggling section visibility
        protected bool IsPetsSectionVisible { get; set; } = true; // Default to visible when component is separate

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await LoadDropdownData();
        }

        private async Task LoadData()
        {
            Pets = await PetService.GetAllAsync();

            // Clamp current pages within the available page counts
            PetsCurrentPage = System.Math.Clamp(PetsCurrentPage, 1, PetsTotalPages == 0 ? 1 : PetsTotalPages);
        }

        private async Task LoadDropdownData()
        {
            AvailableSpecies = await PetService.GetAllSpeciesAsync();
            AvailableUsers = new List<ApplicationUser>(UserManager.Users);
            AvailableBreeds = new List<Breed>(); // Ensure it's explicitly initialized for safety.
        }

        // Handle Species Dropdown Change
        protected async Task OnSpeciesChanged(ChangeEventArgs e)
        {
            PetFormModel.SpeciesId = e.Value?.ToString(); // Update the SpeciesId in the model
            PetFormModel.BreedId = null; // Reset BreedId when species changes

            if (!string.IsNullOrEmpty(PetFormModel.SpeciesId))
            {
                AvailableBreeds = await PetService.GetBreedsBySpeciesIdAsync(PetFormModel.SpeciesId);
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
            StateHasChanged(); // Ensure UI updates to clear breed dropdown
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
                AvailableBreeds = await PetService.GetBreedsBySpeciesIdAsync(PetFormModel.SpeciesId);
            }
            else
            {
                AvailableBreeds = new List<Breed>();
            }
            StateHasChanged(); // Force UI update after breeds are loaded for editing
        }

        protected async Task HandlePetFormSubmit()
        {
            if (IsEditing)
            {
                var existingPet = await PetService.GetByIdAsync(PetFormModel.Id);
                if (existingPet != null)
                {
                    existingPet.PetName = PetFormModel.PetName;
                    existingPet.Age = PetFormModel.Age;

                    // Update Species and Breed IDs
                    existingPet.SpeciesId = PetFormModel.SpeciesId;
                    existingPet.BreedId = PetFormModel.BreedId;
                    existingPet.OwnerId = PetFormModel.OwnerId;

                    await PetService.UpdateAsync(existingPet);
                }
            }
            else
            {
                await PetService.AddAsync(PetFormModel);
            }

            IsPetFormVisible = false;
            await LoadData(); // Reload all data including updated pet list
            StateHasChanged(); // Ensure UI updates after form submission
        }

        protected void CancelPetForm()
        {
            IsPetFormVisible = false;
            StateHasChanged(); // Ensure UI updates to hide form
        }

        protected async Task DeletePet(string id)
        {
            await PetService.DeleteAsync(id);
            await LoadData();
            StateHasChanged(); // Ensure UI updates after deletion
        }

        // ---------- Pagination Handlers ----------
        protected void ChangePetsPage(int page)
        {
            PetsCurrentPage = page < 1 ? 1 : page > PetsTotalPages ? PetsTotalPages : page;
            StateHasChanged(); // Ensure UI updates for pagination
        }

        // ---------- Section Visibility Toggle Methods ----------
        protected void TogglePetsSection()
        {
            IsPetsSectionVisible = !IsPetsSectionVisible;
            StateHasChanged(); // Ensure UI updates for section visibility
        }
    }
}