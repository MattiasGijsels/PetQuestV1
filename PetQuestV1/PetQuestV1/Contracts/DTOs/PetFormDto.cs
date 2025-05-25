using System.ComponentModel.DataAnnotations;

namespace PetQuestV1.Contracts.DTOs.Pets
{
    public class PetFormDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Pet Name is required.")]
        [StringLength(100, ErrorMessage = "Pet Name cannot exceed 100 characters.")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Species is required.")]
        public string? SpeciesId { get; set; }

        public string? BreedId { get; set; }

        [Required(ErrorMessage = "Owner is required.")]
        public string? OwnerId { get; set; }

        [Range(0.0, 30.0, ErrorMessage = "Age must be between 0.0 and 30.0.")]
        public double? Age { get; set; } // Made nullable because the int would let me display a placeholder in the UI

    }
}