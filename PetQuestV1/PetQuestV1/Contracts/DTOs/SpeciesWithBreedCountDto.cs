namespace PetQuestV1.Contracts.Models
{
    public class SpeciesWithBreedCountDto
    {
        public string Id { get; set; } = string.Empty;
        public string SpeciesName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public int BreedCount { get; set; } 
    }
}