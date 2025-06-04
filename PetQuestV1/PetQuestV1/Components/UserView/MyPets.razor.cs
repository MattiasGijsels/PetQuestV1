using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms; // For IBrowserFile
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

using System;

namespace PetQuestV1.Components.UserView
{
    public partial class MyPets : ComponentBase
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!; // For scoped services in singleton component

        protected List<Pet> UserPets { get; set; } = new List<Pet>();
        protected bool isLoading { get; set; } = true;

        //Pet Modal properties
        protected bool showCreateModal { get; set; } = false;
        protected bool isCreating { get; set; } = false;
        protected bool showSuccessAlert { get; set; } = false;
        protected string errorMessage { get; set; } = string.Empty;

        protected PetFormDto newPetDto { get; set; } = new PetFormDto();
        protected List<Species>? allSpecies { get; set; }
        protected List<Breed>? availableBreeds { get; set; }

        private string? _currentUserId;

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                _currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (_currentUserId != null)
                {
                    await LoadUserPets();
                    await LoadSpeciesData(); // Load species for the create form
                }
                else
                {
                    Console.WriteLine("User ID not found in claims.");
                }
            }
            else
            {
                Console.WriteLine("User is not authenticated.");
            }
            isLoading = false;
        }

        private async Task LoadUserPets()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                UserPets = await petService.GetPetsByOwnerIdAsync(_currentUserId!);
            }
        }

        private async Task LoadSpeciesData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                allSpecies = await petService.GetAllSpeciesAsync();
            }
        }

        protected async Task HandleImageUploadRequest((string PetId, IBrowserFile ImageFile) args)// args = the bundle of PetId and ImageFile
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                var uploadedPath = await petService.UploadPetImageAsync(args.PetId, args.ImageFile);

                if (uploadedPath != null)
                {
                    // Update the specific pet in the UserPets list
                    var petToUpdate = UserPets.FirstOrDefault(p => p.Id == args.PetId);
                    if (petToUpdate != null)
                    {
                        petToUpdate.ImagePath = uploadedPath;
                        StateHasChanged(); 
                    }
                }
                else
                {
                    Console.WriteLine($"Image upload failed for pet ID: {args.PetId}");
                }
            }
        }

        protected async Task HandleImageDeleteRequest(string petId)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                bool success = await petService.DeletePetImageAsync(petId);

                if (success)
                {
                    // Clear the image path for the specific pet in the list
                    var petToUpdate = UserPets.FirstOrDefault(p => p.Id == petId);
                    if (petToUpdate != null)
                    {
                        petToUpdate.ImagePath = null;
                        StateHasChanged(); 
                    }
                }
                else
                {
                    Console.WriteLine($"Image deletion failed for pet ID: {petId}");
                }
            }
        }

        protected void ShowCreatePetModal()
        {
            // Reset the form and state
            newPetDto = new PetFormDto
            {
                OwnerId = _currentUserId
                // Advantage will default to 5 from the PetFormDto class
            };
            availableBreeds = new List<Breed>(); // Reset breeds to empty list (not null)
            errorMessage = string.Empty;
            showCreateModal = true;
        }

        protected void HideCreatePetModal()
        {
            showCreateModal = false;
            newPetDto = new PetFormDto();
            availableBreeds = null;
            errorMessage = string.Empty;
        }
        protected async Task OnSpeciesChangedAsync()
        {
            var selectedSpeciesId = newPetDto.SpeciesId;
            newPetDto.BreedId = null; // Reset breed selection

            if (!string.IsNullOrEmpty(selectedSpeciesId))
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                    availableBreeds = await petService.GetBreedsBySpeciesIdAsync(selectedSpeciesId);
                }
            }
            else
            {
                availableBreeds = new List<Breed>(); // Empty list instead of null
            }

            StateHasChanged();
        }

        protected async Task CreateNewPet()
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                errorMessage = "User authentication error. Please refresh and try again.";
                return;
            }

            isCreating = true;
            errorMessage = string.Empty;

            try
            {
                // Ensure the OwnerId is set
                newPetDto.OwnerId = _currentUserId;

                using (var scope = ScopeFactory.CreateScope())
                {
                    var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                    await petService.AddPetAsync(newPetDto);
                }

                // Reload the pets list to include the new pet
                await LoadUserPets();

                // Show success and hide modal
                showSuccessAlert = true;
                HideCreatePetModal();

                // Auto-hide success alert after 5 seconds
                _ = Task.Delay(5000).ContinueWith(_ =>
                {
                    showSuccessAlert = false;
                    InvokeAsync(StateHasChanged);
                });
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to create pet: {ex.Message}";
                Console.WriteLine($"Error creating pet: {ex}");
            }
            finally
            {
                isCreating = false;
                StateHasChanged();
            }
        }
    }
}