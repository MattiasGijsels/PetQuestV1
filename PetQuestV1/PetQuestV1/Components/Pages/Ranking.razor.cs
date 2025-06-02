// Pages/Ranking.razor.cs
using Microsoft.AspNetCore.Components;
using PetQuestV1.Contracts.Defines; // Keep this for IPetService
using PetQuestV1.Contracts.Models; // Keep this for Pet
using System; // Added for Math.Ceiling
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Components.Pages
{   
    public partial class Ranking : ComponentBase
    {

        private List<Pet>? _allPets; // Store all pets initially
        public List<Pet>? DisplayedPets { get; set; } // Pets for the current page

        public int CurrentPage { get; set; } = 1;
        private const int PageSize = 10; // Number of pets per page
        public int TotalPages { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // PetService is directly accessible here without explicit declaration in this file
            _allPets = await PetService.GetAllAsync();
            _allPets = _allPets.OrderByDescending(p => p.Advantage).ToList();

            CalculateTotalPages();
            LoadPage();
        }

        private void CalculateTotalPages()
        {
            if (_allPets != null)
            {
                TotalPages = (int)Math.Ceiling(_allPets.Count / (double)PageSize);
            }
            else
            {
                TotalPages = 0;
            }
        }

        private void LoadPage()
        {
            if (_allPets == null)
            {
                DisplayedPets = new List<Pet>();
                return;
            }

            var skip = (CurrentPage - 1) * PageSize;
            DisplayedPets = _allPets.Skip(skip).Take(PageSize).ToList();
        }

        public void ChangePage(int pageNumber)
        {
            if (pageNumber < 1)
                CurrentPage = 1;
            else if (pageNumber > TotalPages)
                CurrentPage = TotalPages;
            else
                CurrentPage = pageNumber;

            LoadPage();
        }
    }
}