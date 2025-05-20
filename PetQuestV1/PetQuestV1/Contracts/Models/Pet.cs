using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetQuestV1.Contracts.Shared; 
using PetQuestV1.Data; 

namespace PetQuestV1.Contracts.Models
{
    public class Pet : ModelBase
    {
        [Required(ErrorMessage = "Pet Name is required.")]
        [StringLength(100, ErrorMessage = "Pet Name must be less than 100 characters.")]
        public string PetName { get; set; } = default!;

        [Required(ErrorMessage = "Species is required.")] // pet MUST have a species
        public string SpeciesId { get; set; } = default!;

        [ForeignKey("SpeciesId")]
        // Make Species navigation property nullable if SpeciesId can be null.
        // It's good practice for the navigation property to match the nullability of its FK.
        public Species? Species { get; set; }

        [Required(ErrorMessage = "Breed is required.")]
        [StringLength(50, ErrorMessage = "Breed must be less than 50 characters.")]
        public string Breed { get; set; } = default!;

        public int Advantage { get; set; } = 5;

        [Range(0, 100, ErrorMessage = "Age must be between 0 and 100.")] 
        public int Age { get; set; }

        // Owner relationship
        // Make OwnerId nullable if a pet might not have an owner initially,maybe if there is time for adoption module
        // or if it can be unset via the dropdown.
        [Required(ErrorMessage = "Owner is required.")] // Keep this if a pet MUST have an owner
        public string OwnerId { get; set; } = default!;

        [ForeignKey("OwnerId")]
        // Make Owner navigation property nullable if OwnerId can be null.
        public ApplicationUser? Owner { get; set; }

        public Pet() : base() { }
    }
}