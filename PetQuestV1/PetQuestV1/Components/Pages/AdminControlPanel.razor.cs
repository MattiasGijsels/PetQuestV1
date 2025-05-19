using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Components.Pages
{
    public partial class AdminControlPanelBase : ComponentBase
    {
        [Inject] public IPetService PetService { get; set; } = default!;
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; } = default!;

        protected List<Pet> Pets { get; set; } = new();
        protected List<ApplicationUser> Users { get; set; } = new();

        protected Pet PetFormModel { get; set; } = new();
        protected bool IsPetFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            Pets = await PetService.GetAllAsync();
            Users = new List<ApplicationUser>(UserManager.Users); // Sync query for demo; better to do async in production
        }

        protected void ShowAddPetForm()
        {
            PetFormModel = new Pet();
            IsEditing = false;
            IsPetFormVisible = true;
        }

        protected void EditPet(Pet pet)
        {
            PetFormModel = new Pet
            {
                Id = pet.Id,
                PetName = pet.PetName,
                SpeciesId = pet.SpeciesId,
                OwnerId = pet.OwnerId
            };
            IsEditing = true;
            IsPetFormVisible = true;
        }

        protected async Task HandlePetFormSubmit()
        {
            if (IsEditing)
                await PetService.UpdateAsync(PetFormModel);
            else
                await PetService.AddAsync(PetFormModel);

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
    }
}
