// Components/Pages/AdminControlPanelBase.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Components.Pages
{
    public partial class AdminControlPanelBase : ComponentBase
    {
        [Inject] public IPetService PetService { get; set; } = default!;
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; } = default!;

        protected List<Pet> Pets { get; set; } = new();
        protected List<ApplicationUser> Users { get; set; } = new();
        protected List<Species> AvailableSpecies { get; set; } = new(); // To hold all species for dropdown
        protected List<Breed> AvailableBreeds { get; set; } = new();   // --- NEW: To hold breeds for dependent dropdown ---
        protected List<ApplicationUser> AvailableUsers { get; set; } = new(); // To hold all users for dropdown

        // Pagination pets
        protected int PetsCurrentPage { get; set; } = 1;
        protected int PetsPageSize { get; set; } = 10;
        protected int PetsTotalPages => (int)System.Math.Ceiling((double)Pets.Count / PetsPageSize);
        protected IEnumerable<Pet> PagedPets => Pets.Skip((PetsCurrentPage - 1) * PetsPageSize).Take(PetsPageSize);

        // Pagination users
        protected int UsersCurrentPage { get; set; } = 1;
        protected int UsersPageSize { get; set; } = 10;
        protected int UsersTotalPages => (int)System.Math.Ceiling((double)Users.Count / UsersPageSize);
        protected IEnumerable<ApplicationUser> PagedUsers => Users.Skip((UsersCurrentPage - 1) * UsersPageSize).Take(UsersPageSize);

        // Forms & UI state for pets management
        protected Pet PetFormModel { get; set; } = new();
        protected bool IsPetFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;

        // Toggling section visibility (initially set to false)
        protected bool IsPetsSectionVisible { get; set; } = false;
        protected bool IsUsersSectionVisible { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await LoadDropdownData(); // Load data for dropdowns
        }

        private async Task LoadData()
        {
            Pets = await PetService.GetAllAsync();
            Users = new List<ApplicationUser>(UserManager.Users);

            // Clamp current pages within the available page counts
            PetsCurrentPage = System.Math.Clamp(PetsCurrentPage, 1, PetsTotalPages == 0 ? 1 : PetsTotalPages);
            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
        }

        private async Task LoadDropdownData()
        {
            AvailableSpecies = await PetService.GetAllSpeciesAsync();
            AvailableUsers = new List<ApplicationUser>(UserManager.Users);
            // Initially load all breeds or none, depending on preference.
            // When adding a new pet, we don't have a species selected yet, so load all or empty.
            // When editing, we'll load based on the existing pet's species.
            AvailableBreeds = new List<Breed>(); // Start with an empty list for breeds initially
        }

        // --- NEW: Handle Species Dropdown Change ---
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
        }

        // ---------- Pets CRUD Handlers ----------
        protected void ShowAddPetForm()
        {
            PetFormModel = new Pet(); // New pet
            IsEditing = false;
            IsPetFormVisible = true;
            AvailableBreeds = new List<Breed>(); // Clear breeds for new pet form
        }

        protected async Task EditPet(Pet pet) // Changed to async to load breeds
        {
            // Assign existing values for editing
            PetFormModel = new Pet
            {
                Id = pet.Id,
                PetName = pet.PetName,
                SpeciesId = pet.Species?.Id,
                OwnerId = pet.Owner?.Id,
                BreedId = pet.Breed?.Id, // --- NEW: Assign BreedId from existing pet ---
                Age = pet.Age
            };
            IsEditing = true;
            IsPetFormVisible = true;

            // --- NEW: Load breeds based on the existing pet's species for editing ---
            if (!string.IsNullOrEmpty(PetFormModel.SpeciesId))
            {
                AvailableBreeds = await PetService.GetBreedsBySpeciesIdAsync(PetFormModel.SpeciesId);
            }
            else
            {
                AvailableBreeds = new List<Breed>();
            }
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
                    existingPet.BreedId = PetFormModel.BreedId; // --- NEW: Assign BreedId ---
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
        }

        protected void CancelPetForm()
        {
            IsPetFormVisible = false;
        }

        protected async Task DeletePet(string id)
        {
            await PetService.DeleteAsync(id);
            await LoadData();
        }

        // ---------- Pagination Handlers ----------
        protected void ChangePetsPage(int page)
        {
            PetsCurrentPage = page < 1 ? 1 : page > PetsTotalPages ? PetsTotalPages : page;
        }

        protected void ChangeUsersPage(int page)
        {
            UsersCurrentPage = page < 1 ? 1 : page > UsersTotalPages ? UsersTotalPages : page;
        }

        // ---------- Section Visibility Toggle Methods ----------
        protected void TogglePetsSection()
        {
            IsPetsSectionVisible = !IsPetsSectionVisible;
        }

        protected void ToggleUsersSection()
        {
            IsUsersSectionVisible = !IsPetsSectionVisible;
        }
    }
}