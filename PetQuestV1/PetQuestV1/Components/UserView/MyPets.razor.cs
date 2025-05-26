using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Defines;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity; // For UserManager
using Microsoft.AspNetCore.Components.Forms; // For IBrowserFile
using System; // For Console.WriteLine

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

        protected async Task HandleImageUploadRequest((string PetId, IBrowserFile ImageFile) args)
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
                        StateHasChanged(); // Notify Blazor to re-render the UI
                    }
                }
                else
                {
                    Console.WriteLine($"Image upload failed for pet ID: {args.PetId}");
                    // Optionally, add user feedback (e.g., a toast notification)
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
                        StateHasChanged(); // Notify Blazor to re-render the UI
                    }
                }
                else
                {
                    Console.WriteLine($"Image deletion failed for pet ID: {petId}");
                    // Optionally, add user feedback
                }
            }
        }
    }
}