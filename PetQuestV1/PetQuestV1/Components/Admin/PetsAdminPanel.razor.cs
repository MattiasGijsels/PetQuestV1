using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Enums;
using PetQuestV1.Contracts.DTOs.Pets;
using Microsoft.AspNetCore.Components.Forms; // Add this using statement
using System;
using System.IO;
using System.Linq; // Added for .Any() and other LINQ methods
using System.Collections.Generic; // Added for List<T>
using System.Threading.Tasks;

namespace PetQuestV1.Components.Admin
{
    public partial class PetsAdminPanelBase : ComponentBase
    {
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        // No longer need IWebHostEnvironment directly here, as PetService handles it.
        // [Inject]
        // private IWebHostEnvironment Env { get; set; } = default!;

        protected List<Pet> AllPets { get; set; } = new();
        protected List<Species> AvailableSpecies { get; set; } = new();
        protected List<Breed> AvailableBreeds { get; set; } = new();
        protected List<ApplicationUser> AvailableUsers { get; set; } = new();
        protected string CurrentSortColumn { get; set; } = "PetName";
        protected SortDirection SortDirection { get; set; } = SortDirection.Ascending;
        protected string SearchTerm { get; set; } = string.Empty;
        protected int PetsCurrentPage { get; set; } = 1;
        protected int PetsPageSize { get; set; } = 10;
        protected PetFormDto PetFormModel { get; set; } = new();
        protected bool IsPetFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;
        protected bool IsPetsSectionVisible { get; set; } = false;

        // Filtered and Sorted Pets logic
        protected IEnumerable<Pet> PagedPets => FilteredAndSortedPets.Skip((PetsCurrentPage - 1) * PetsPageSize).Take(PetsPageSize);
        protected IEnumerable<Pet> FilteredAndSortedPets
        {
            get
            {
                var query = AllPets.Where(p => !p.IsDeleted).AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(p =>
                        p.PetName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (p.Species != null && !string.IsNullOrEmpty(p.Species.SpeciesName) && p.Species.SpeciesName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Breed != null && !string.IsNullOrEmpty(p.Breed.BreedName) && p.Breed.BreedName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Owner != null && !string.IsNullOrEmpty(p.Owner.UserName) && p.Owner.UserName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    );
                }

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
                    _ => query.OrderBy(p => p.PetName)
                };

                return query.ToList();
            }
        }
        protected int PetsTotalPages
        {
            get
            {
                int totalPets = FilteredAndSortedPets.Count();
                if (totalPets == 0)
                {
                    return 1;
                }
                return (int)Math.Ceiling((double)totalPets / PetsPageSize);
            }
        }

        // --- NEW: Property to hold the selected file from InputFile ---
        private IBrowserFile? _selectedFile;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await LoadDropdownData();
            PetsCurrentPage = Math.Clamp(PetsCurrentPage, 1, PetsTotalPages);
        }

        private async Task LoadData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                AllPets = await petService.GetAllAsync();
            }
            StateHasChanged();
        }

        private async Task LoadDropdownData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                AvailableSpecies = await petService.GetAllSpeciesAsync();
                AvailableUsers = await userManager.Users.ToListAsync();
                AvailableBreeds = new List<Breed>();
            }
            StateHasChanged();
        }

        protected async Task OnSpeciesChanged(ChangeEventArgs e)
        {
            PetFormModel.SpeciesId = e.Value?.ToString();
            PetFormModel.BreedId = null;

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
            PetFormModel = new PetFormDto();
            IsEditing = false;
            IsPetFormVisible = true;
            AvailableBreeds = new List<Breed>();
            _selectedFile = null; // Clear any previously selected file when adding a new pet
            StateHasChanged();
        }

        protected async Task EditPet(Pet pet)
        {
            PetFormModel = new PetFormDto
            {
                Id = pet.Id,
                PetName = pet.PetName,
                SpeciesId = pet.SpeciesId,
                OwnerId = pet.OwnerId,
                BreedId = pet.BreedId,
                Age = pet.Age,
                ImagePath = pet.ImagePath // Load the existing image path from the pet model
            };
            IsEditing = true;
            IsPetFormVisible = true;
            _selectedFile = null; // Clear any pending file upload when starting an edit

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

        // --- NEW: Method to handle file input change ---
        protected void OnInputFileChange(InputFileChangeEventArgs e)
        {
            _selectedFile = e.File;
            // Optionally, you can add a temporary client-side preview here
            // using e.File.OpenReadStream and converting to base64 for PetFormModel.ImagePath
            // if you want immediate feedback without waiting for server upload.
            // For now, we'll rely on the server upload to update ImagePath.
        }

        // --- NEW: Method to handle image deletion ---
        protected async Task DeleteImage()
        {
            if (PetFormModel.Id != null) // Only delete if it's an existing pet
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                    bool success = await petService.DeletePetImageAsync(PetFormModel.Id);
                    if (success)
                    {
                        PetFormModel.ImagePath = null; // Clear the image path from the form model
                        await LoadData(); // Reload data to ensure the table updates
                        StateHasChanged();
                    }
                    else
                    {
                        // Handle error, e.g., show a toast notification
                        Console.WriteLine("Error deleting pet image.");
                    }
                }
            }
        }

        protected async Task HandlePetFormSubmit()
        {
            if (string.IsNullOrWhiteSpace(PetFormModel.PetName) ||
                string.IsNullOrWhiteSpace(PetFormModel.SpeciesId) ||
                string.IsNullOrWhiteSpace(PetFormModel.OwnerId))
            {
                // Basic validation, DataAnnotationsValidator handles more
                return;
            }

            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();

                if (IsEditing)
                {
                    // Update pet details
                    await petService.UpdatePetAsync(PetFormModel);

                    // If a new file was selected during edit, upload it
                    if (_selectedFile != null && PetFormModel.Id != null)
                    {
                        var uploadedPath = await petService.UploadPetImageAsync(PetFormModel.Id, _selectedFile);
                        if (uploadedPath != null)
                        {
                            PetFormModel.ImagePath = uploadedPath; // Update DTO with new path
                        }
                        _selectedFile = null; // Clear the selected file after upload
                    }
                }
                else // Adding a new pet
                {
                   
                    // For now, let's keep it simple and assume the PetFormModel.Id will be set after Add.
                    // A better approach for new pets with images is often a dedicated DTO or a sequence of calls.

                    // Simplest approach: Add the pet, then fetch it to get its ID, then upload.
                    // This requires a PetFormDto that can become a Pet, or fetch by properties.
                    // Given PetFormDto has an Id, we will assume it's assigned after AddAsync
                    // in the PetRepository if it's null, which it currently is.
                    // Let's make AddPetAsync in PetService return the created pet's Id.

                    await petService.AddPetAsync(PetFormModel); // This will generate an ID if null

                    // After adding, if an ID was assigned (which it is by PetRepository if null)
                    // and a file was selected, upload the image.
                    if (_selectedFile != null && !string.IsNullOrEmpty(PetFormModel.Id))
                    {
                        var uploadedPath = await petService.UploadPetImageAsync(PetFormModel.Id, _selectedFile);
                        if (uploadedPath != null)
                        {
                            PetFormModel.ImagePath = uploadedPath; // Update DTO with new path
                        }
                        _selectedFile = null; // Clear the selected file after upload
                    }
                }
            }

            IsPetFormVisible = false;
            PetFormModel = new PetFormDto(); // Reset form model
            await LoadData(); // Reload all pets including new image paths
            StateHasChanged();
        }

        protected void CancelPetForm()
        {
            IsPetFormVisible = false;
            PetFormModel = new PetFormDto();
            _selectedFile = null; // Clear selected file on cancel
            StateHasChanged();
        }

        protected async Task SoftDeletePet(string id)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                await petService.SoftDeleteAsync(id);
                // Consider also deleting the image from storage if soft deleting means it's truly gone forever
                // For now, soft delete only marks the DB record. If you want to delete the file:
                // await petService.DeletePetImageAsync(id); // <--- UNCOMMENT THIS IF YOU WANT PHYSICAL FILE DELETION ON SOFT DELETE
            }
            await LoadData();
            StateHasChanged();
        }

        protected void ChangePetsPage(int page)
        {
            PetsCurrentPage = Math.Clamp(page, 1, PetsTotalPages);
            StateHasChanged();
        }

        protected void TogglePetsSection()
        {
            IsPetsSectionVisible = !IsPetsSectionVisible;
            if (!IsPetsSectionVisible)
            {
                IsPetFormVisible = false;
                PetFormModel = new PetFormDto();
                _selectedFile = null; // Clear selected file on section collapse
            }
            StateHasChanged();
        }

        protected void SortBy(string columnName)
        {
            if (CurrentSortColumn == columnName)
            {
                SortDirection = (SortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                CurrentSortColumn = columnName;
                SortDirection = SortDirection.Ascending;
            }
            PetsCurrentPage = 1;
            StateHasChanged();
        }

        protected string GetSortIcon(string columnName)
        {
            if (CurrentSortColumn != columnName)
            {
                return "bi-sort-alpha-down";
            }
            return SortDirection == SortDirection.Ascending ? "bi-sort-alpha-down" : "bi-sort-alpha-up";
        }

        protected void OnSearchInput(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? string.Empty;
            PetsCurrentPage = 1;
            StateHasChanged();
        }
    }
}