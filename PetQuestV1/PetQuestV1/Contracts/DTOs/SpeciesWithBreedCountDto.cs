// PetQuestV1/Contracts/Models/SpeciesWithBreedCountDto.cs
// This is a DTO (Data Transfer Object) specifically for the UI display
// It holds species information plus a derived property (BreedCount)
namespace PetQuestV1.Contracts.Models
{
    public class SpeciesWithBreedCountDto
    {
        public string Id { get; set; } = string.Empty;
        public string SpeciesName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public int BreedCount { get; set; } // <--- NEW PROPERTY
    }
}