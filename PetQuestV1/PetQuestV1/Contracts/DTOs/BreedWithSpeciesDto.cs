// PetQuestV1/Contracts/Models/BreedWithSpeciesDto.cs
namespace PetQuestV1.Contracts.Models
{
    public class BreedWithSpeciesDto
    {
        public string Id { get; set; } = string.Empty;
        public string BreedName { get; set; } = string.Empty;
        public string SpeciesName { get; set; } = string.Empty; // To display species name in the list
        public string SpeciesId { get; set; } = string.Empty; // To keep track of the species ID
        public bool IsDeleted { get; set; }
    }
}