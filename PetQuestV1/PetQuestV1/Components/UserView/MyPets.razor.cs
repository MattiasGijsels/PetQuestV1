using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Defines; // For IPetService
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
// using Microsoft.AspNetCore.Identity; // Only if UserManager is directly used here. PetService handles user association.
using Microsoft.AspNetCore.Components.Forms; // For IBrowserFile
using System; // For Console.WriteLine

namespace PetQuestV1.Components.UserView
{
    public partial class MyPets : ComponentBase
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        protected List<Pet> UserPets { get; set; } = new List<Pet>();
        protected bool isLoading { get; set; } = true;
        private string? _currentUserId;

        // For user feedback
        protected string? UserActionMessage { get; set; }
        protected bool IsUserActionSuccess { get; set; }


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
                }
                else
                {
                    Console.WriteLine("User ID not found in claims.");
                    UserActionMessage = "Could not identify user. Please try logging in again.";
                    IsUserActionSuccess = false;
                }
            }
            else
            {
                Console.WriteLine("User is not authenticated.");
                UserActionMessage = "You must be logged in to view your pets.";
                IsUserActionSuccess = false;
            }
            isLoading = false;
        }

        private async Task LoadUserPets()
        {
            UserActionMessage = null; // Clear previous messages
            if (string.IsNullOrEmpty(_currentUserId)) return;

            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                UserPets = await petService.GetPetsByOwnerIdAsync(_currentUserId);
            }
        }

        protected async Task HandleImageUploadRequest((string PetId, IBrowserFile ImageFile) args)
        {
            UserActionMessage = null; // Clear previous messages
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                var uploadedPath = await petService.UploadPetImageAsync(args.PetId, args.ImageFile);

                if (uploadedPath != null)
                {
                    var petToUpdate = UserPets.FirstOrDefault(p => p.Id == args.PetId);
                    if (petToUpdate != null)
                    {
                        petToUpdate.ImagePath = uploadedPath;
                        UserActionMessage = $"Image for {petToUpdate.PetName} uploaded successfully!";
                        IsUserActionSuccess = true;
                        StateHasChanged();
                    }
                }
                else
                {
                    Console.WriteLine($"MyPets: Image upload failed for pet ID: {args.PetId}. 'uploadedPath' was null.");
                    UserActionMessage = "Image upload failed. Please check server logs or try a different image.";
                    IsUserActionSuccess = false;
                    StateHasChanged();
                }
            }
        }

        protected async Task HandleImageDeleteRequest(string petId)
        {
            UserActionMessage = null; // Clear previous messages
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                bool success = await petService.DeletePetImageAsync(petId);
                var petToUpdate = UserPets.FirstOrDefault(p => p.Id == petId);


                if (success && petToUpdate != null)
                {
                    petToUpdate.ImagePath = null;
                    UserActionMessage = $"Image for {petToUpdate.PetName} deleted successfully.";
                    IsUserActionSuccess = true;
                    StateHasChanged();
                }
                else
                {
                    var petName = petToUpdate?.PetName ?? "the selected pet";
                    Console.WriteLine($"MyPets: Image deletion failed for pet ID: {petId}");
                    UserActionMessage = $"Failed to delete image for {petName}.";
                    IsUserActionSuccess = false;
                    StateHasChanged();
                }
            }
        }
    }
}