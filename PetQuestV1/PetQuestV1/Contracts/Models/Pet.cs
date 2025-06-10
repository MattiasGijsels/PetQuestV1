using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts.Shared;
using PetQuestV1.Data;

namespace PetQuestV1.Contracts.Models
{
    public class Pet : ModelBase
    {
        [Required]
        [StringLength(100)]
        public string PetName { get; set; } = default!;

        
        public string? SpeciesId { get; set; } = default!;

        [ForeignKey("SpeciesId")]
        public Species? Species { get; set; }

        public string? BreedId { get; set; } = default!; 

        [ForeignKey("BreedId")]
        public Breed? Breed { get; set; } 

        public int Advantage { get; set; } = 5;

        [Range(0.0, 100.0, ErrorMessage = "Age must be between 0 and 100.")]
        public double? Age { get; set; }

        public string? OwnerId { get; set; } = default!; 

        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; }

        public string? ImagePath { get; set; }  

        public Pet() : base() { }
    }
}