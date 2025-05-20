using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetQuestV1.Contracts.Shared;

namespace PetQuestV1.Contracts.Models
{
    public class Breed : ModelBase
    {
        [Required]
        [StringLength(100)]
        public string BreedName { get; set; } = default!;

        [Required]
        public string SpeciesId { get; set; } = default!; // FK to Species

        [ForeignKey("SpeciesId")]
        public Species? Species { get; set; } // Navigation property

        public Breed() : base() { }
    }
}