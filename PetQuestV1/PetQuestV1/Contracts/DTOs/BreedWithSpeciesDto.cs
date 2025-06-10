namespace PetQuestV1.Contracts.DTOs
{
    public class BreedWithSpeciesDto
    {
        public string Id { get; set; } = string.Empty;
        public string BreedName { get; set; } = string.Empty;
        public string SpeciesName { get; set; } = string.Empty; 
        public string SpeciesId { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}