using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts.Shared;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;

namespace PetQuestV1.Contracts.Models
{
    public class Pet : ModelBase
    {
        [Required]
        [StringLength(100)]
        public string PetName { get; set; } = default!;

        [Required]
        public string SpeciesId { get; set; } = default!; // FK to Species

        [ForeignKey("SpeciesId")]
        public Species? Species { get; set; }
 
        // It's good practice for the navigation property to match the nullability of its FK
        [Required]
        [StringLength(50)]
        public string Breed { get; set; } = default!;

        public int Advantage { get; set; } = 5;

        // --- CHANGE THIS LINE ---
        [Range(0.0, 100.0, ErrorMessage = "Age must be between 0 and 100.")]
        public double? Age { get; set; } // Changed from double to double? (nullable)
        // Note: [Required] is intentionally NOT added here if you want the placeholder to show.
        // If age is mandatory, you'll rely on the [Range] validation.

        // Foreign Key to the User
        [Required]
        public string OwnerId { get; set; } = default!;

        // Navigation Property to the User
        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; } // Made nullable as per previous discussion

        public Pet() : base() { }
    }
}