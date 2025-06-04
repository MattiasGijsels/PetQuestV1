namespace PetQuestV1.Contracts.DTOs
{
    public class PetViewerDto
    {
        public string? Id { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string? SpeciesName { get; set; }
        public string? BreedName { get; set; }
        public string? OwnerName { get; set; }
        public double? Age { get; set; }
        public int Advantage { get; set; }
        public string? ImagePath { get; set; }
    }
}
