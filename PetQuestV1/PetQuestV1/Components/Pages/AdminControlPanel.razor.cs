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
        }

        private async Task LoadData()
        {
            Pets = await PetService.GetAllAsync();
            Users = new List<ApplicationUser>(UserManager.Users);

            // Clamp current pages within the available page counts
            PetsCurrentPage = System.Math.Clamp(PetsCurrentPage, 1, PetsTotalPages == 0 ? 1 : PetsTotalPages);
            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
        }

        // ---------- Pets CRUD Handlers ----------
        protected void ShowAddPetForm()
        {
            PetFormModel = new Pet();
            IsEditing = false;
            IsPetFormVisible = true;
        }

        // In PetQuestV1.Components.Pages.AdminControlPanelBase
        protected void EditPet(Pet pet)
        {
            // Deep copy the pet object to ensure changes in the form
            // don't directly modify the item in the displayed list before saving.
            PetFormModel = new Pet
            {
                Id = pet.Id,
                PetName = pet.PetName,
                // Ensure Species and Owner objects are not null and their properties are correctly assigned
                Species = new Species { Id = pet.Species?.Id, Name = pet.Species?.Name },
                Owner = new ApplicationUser { Id = pet.Owner?.Id, UserName = pet.Owner?.UserName }
            };
            IsEditing = true;
            IsPetFormVisible = true;
        }

        // In PetQuestV1.Components.Pages.AdminControlPanelBase
        protected async Task HandlePetFormSubmit()
        {
            // Handle Species
            if (!string.IsNullOrWhiteSpace(PetFormModel.Species?.Name))
            {
                // Try to find an existing species by name
                var existingSpecies = await PetService.GetSpeciesByNameAsync(PetFormModel.Species.Name);
                if (existingSpecies != null)
                {
                    PetFormModel.Species = existingSpecies;
                }
                else if (string.IsNullOrWhiteSpace(PetFormModel.Species.Id)) // Only add if it's truly new
                {
                    // If not found and it's a new pet or a new species for an existing pet, create a new Species object with a new ID
                    PetFormModel.Species.Id = Guid.NewGuid().ToString(); // Generate a new ID for a new species
                }
            }
            else
            {
                PetFormModel.Species = null; // Clear species if no name is provided
            }

            // Handle Owner
            if (!string.IsNullOrWhiteSpace(PetFormModel.Owner?.UserName))
            {
                // Try to find an existing user by UserName
                var existingOwner = await UserManager.FindByNameAsync(PetFormModel.Owner.UserName);
                if (existingOwner != null)
                {
                    PetFormModel.Owner = existingOwner;
                }
                else
                {
                    // If the owner's username is provided but doesn't exist, you might want to show an error
                    // or handle it differently. For now, setting it to null or the provided username might be an option
                    // depending on your business logic. For this example, we'll assume it must exist.
                    PetFormModel.Owner = null; // Or throw an error/validation message
                }
            }
            else
            {
                PetFormModel.Owner = null; // Clear owner if no username is provided
            }

            if (IsEditing)
            {
                // For updating, ensure you fetch the original pet and update its properties
                // to maintain references if EF Core is tracking them.
                var originalPet = Pets.FirstOrDefault(p => p.Id == PetFormModel.Id);
                if (originalPet != null)
                {
                    originalPet.PetName = PetFormModel.PetName;
                    originalPet.Species = PetFormModel.Species;
                    originalPet.Owner = PetFormModel.Owner;
                    await PetService.UpdateAsync(originalPet);
                }
            }
            else
            {
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
