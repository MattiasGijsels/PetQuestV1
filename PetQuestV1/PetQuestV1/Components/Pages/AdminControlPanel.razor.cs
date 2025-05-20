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
            AvailableSpecies = await PetService.GetAllSpeciesAsync(); // New service method
            AvailableUsers = new List<ApplicationUser>(UserManager.Users);
        }

        // ---------- Pets CRUD Handlers ----------
        protected void ShowAddPetForm()
        {
            PetFormModel = new Pet(); // New pet, IDs will be generated on save if needed
            IsEditing = false;
            IsPetFormVisible = true;
        }

        protected void EditPet(Pet pet)
        {
            // Assign existing IDs for dropdown selection
            PetFormModel = new Pet
            {
                Id = pet.Id,
                PetName = pet.PetName,
                SpeciesId = pet.Species?.Id, // Bind to SpeciesId
                OwnerId = pet.Owner?.Id      // Bind to OwnerId
            };
            IsEditing = true;
            IsPetFormVisible = true;
        }

        protected async Task HandlePetFormSubmit()
        {
            if (IsEditing)
            {
                // Fetch the existing pet to update its navigation properties
                var existingPet = await PetService.GetByIdAsync(PetFormModel.Id);
                if (existingPet != null)
                {
                    existingPet.PetName = PetFormModel.PetName;

                    // Update Species based on selected SpeciesId
                    existingPet.SpeciesId = PetFormModel.SpeciesId;
                    existingPet.Species = PetFormModel.SpeciesId != null
                        ? AvailableSpecies.FirstOrDefault(s => s.Id == PetFormModel.SpeciesId)
                        : null;

                    // Update Owner based on selected OwnerId
                    existingPet.OwnerId = PetFormModel.OwnerId;
                    existingPet.Owner = PetFormModel.OwnerId != null
                        ? AvailableUsers.FirstOrDefault(u => u.Id == PetFormModel.OwnerId)
                        : null;

                    await PetService.UpdateAsync(existingPet);
                }
            }
            else
            {
                // For adding a new pet, ensure Species and Owner objects are populated
                // based on the selected IDs from the dropdowns.
                PetFormModel.Species = PetFormModel.SpeciesId != null
                    ? AvailableSpecies.FirstOrDefault(s => s.Id == PetFormModel.SpeciesId)
                    : null;
                PetFormModel.Owner = PetFormModel.OwnerId != null
                    ? AvailableUsers.FirstOrDefault(u => u.Id == PetFormModel.OwnerId)
                    : null;

                await PetService.AddAsync(PetFormModel);
            }

            IsPetFormVisible = false;
            await LoadData();
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
            IsUsersSectionVisible = !IsUsersSectionVisible;
        }
    }
}