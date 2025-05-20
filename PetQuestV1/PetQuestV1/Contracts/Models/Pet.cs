using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Make sure this is present if using [ForeignKey]
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts.Shared;
using PetQuestV1.Data; // Ensure this is correct for ApplicationUser

namespace PetQuestV1.Contracts.Models
{
    public class Pet : ModelBase
    {
        [Required]
        [StringLength(100)]
        public string PetName { get; set; } = default!;

        // REMOVE [Required] if you want to use OnDelete(DeleteBehavior.SetNull) for Species
        public string? SpeciesId { get; set; } = default!;

        [ForeignKey("SpeciesId")]
        public Species? Species { get; set; }


        // REMOVE [Required] if you want to use OnDelete(DeleteBehavior.SetNull) for Breed
        public string? BreedId { get; set; } = default!; // FK to Breed

        [ForeignKey("BreedId")]
        public Breed? Breed { get; set; } // Navigation property to Breed

        public int Advantage { get; set; } = 5;

        [Range(0.0, 100.0, ErrorMessage = "Age must be between 0 and 100.")]
        public double? Age { get; set; }

        // REMOVE [Required] if you want to use OnDelete(DeleteBehavior.SetNull) for Owner
        public string? OwnerId { get; set; } = default!; // FK to ApplicationUser

        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; }

        public Pet() : base() { }
    }
}