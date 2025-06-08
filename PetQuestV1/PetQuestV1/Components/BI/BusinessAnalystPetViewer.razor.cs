using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Services;
using System.IO;
using System.Text.Json;

namespace PetQuestV1.Components.BI
{
    public partial class BusinessAnalystPetViewerBase : ComponentBase
    {
        [Inject] public IPetService PetService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IWebHostEnvironment WebHostEnvironment { get; set; } = default!;

        protected List<PetViewerDto> AllPets = new();
        protected List<PetViewerDto> FilteredPets = new();
        protected List<PetViewerDto> PagedPets = new();

        protected string SearchTerm = string.Empty;
        protected int CurrentPage = 1;
        protected int PageSize = 12;
        protected int TotalPages => (int)Math.Ceiling((double)FilteredPets.Count / PageSize);

        protected override async Task OnInitializedAsync()
        {
            AllPets = await PetService.GetAllPetsForAnalystAsync();
            ApplyFilters();
        }

        protected void OnSearchChanged(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? "";
            ApplyFilters();
        }

        protected void ApplyFilters()
        {
            FilteredPets = AllPets.Where(p =>
                (p.PetName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.SpeciesName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.BreedName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.OwnerName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                p.Age?.ToString().Contains(SearchTerm) == true ||
                p.Advantage.ToString().Contains(SearchTerm)
            ).ToList();

            CurrentPage = 1;
            UpdatePagedPets();
        }

        protected void ChangePage(int page)
        {
            CurrentPage = page;
            UpdatePagedPets();
        }

        protected void UpdatePagedPets()
        {
            PagedPets = FilteredPets
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        protected async Task ExportToJson()
        {
            var fileName = $"PetData_{DateTime.Now:yyyyMMddHHmmss}.json";
            var path = Path.Combine(WebHostEnvironment.WebRootPath, fileName);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(AllPets));
            Navigation.NavigateTo($"/{fileName}", true);
        }

        protected async Task ExportToExcel()
        {
            var fileName = $"PetData_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var path = Path.Combine(WebHostEnvironment.WebRootPath, fileName);

            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Pets");

                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Species";
                worksheet.Cell(1, 3).Value = "Breed";
                worksheet.Cell(1, 4).Value = "Age";
                worksheet.Cell(1, 5).Value = "Advantage";
                worksheet.Cell(1, 6).Value = "Owner";

                for (int i = 0; i < AllPets.Count; i++)
                {
                    var pet = AllPets[i];
                    worksheet.Cell(i + 2, 1).Value = pet.PetName;
                    worksheet.Cell(i + 2, 2).Value = pet.SpeciesName;
                    worksheet.Cell(i + 2, 3).Value = pet.BreedName;
                    worksheet.Cell(i + 2, 4).Value = pet.Age;
                    worksheet.Cell(i + 2, 5).Value = pet.Advantage;
                    worksheet.Cell(i + 2, 6).Value = pet.OwnerName;
                }

                workbook.SaveAs(path);
            });

            Navigation.NavigateTo($"/{fileName}", true);
        }

    }
}
