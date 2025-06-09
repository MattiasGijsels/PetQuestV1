using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Forms;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Enums;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Components.Admin
{
    public partial class PetsAdminPanelBase : ComponentBase
    {
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

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
                    "Advantage" => SortDirection == SortDirection.Ascending ? query.OrderBy(p => p.Advantage) : query.OrderByDescending(p => p.Advantage),
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
            _selectedFile = null; 
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
                Advantage = pet.Advantage,
                ImagePath = pet.ImagePath 
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

        protected void OnInputFileChange(InputFileChangeEventArgs e)
        {
            _selectedFile = e.File;
        }

        protected async Task DeleteImage()
        {
            if (PetFormModel.Id != null) 
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                    bool success = await petService.DeletePetImageAsync(PetFormModel.Id);
                    if (success)
                    {
                        PetFormModel.ImagePath = null; 
                        await LoadData(); // Reload data to ensure the table updates
                        StateHasChanged();
                    }
                    else
                    {
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
                return;
            }

            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();

                if (IsEditing)
                {
                    await petService.UpdatePetAsync(PetFormModel);

                    if (_selectedFile != null && PetFormModel.Id != null)
                    {
                        var uploadedPath = await petService.UploadPetImageAsync(PetFormModel.Id, _selectedFile);
                        if (uploadedPath != null)
                        {
                            PetFormModel.ImagePath = uploadedPath; 
                        }
                        _selectedFile = null; 
                    }
                }
                else 
                {

                    await petService.AddPetAsync(PetFormModel); 

                    if (_selectedFile != null && !string.IsNullOrEmpty(PetFormModel.Id))
                    {
                        var uploadedPath = await petService.UploadPetImageAsync(PetFormModel.Id, _selectedFile);
                        if (uploadedPath != null)
                        {
                            PetFormModel.ImagePath = uploadedPath; 
                        }
                        _selectedFile = null;
                    }
                }
            }

            IsPetFormVisible = false;
            PetFormModel = new PetFormDto(); 
            await LoadData(); 
            StateHasChanged();
        }

        protected void CancelPetForm()
        {
            IsPetFormVisible = false;
            PetFormModel = new PetFormDto();
            _selectedFile = null; 
            StateHasChanged();
        }

        protected async Task SoftDeletePet(string id)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                await petService.SoftDeleteAsync(id);
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