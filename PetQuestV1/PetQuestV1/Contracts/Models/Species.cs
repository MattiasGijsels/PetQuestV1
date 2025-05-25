using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Shared;

namespace PetQuestV1.Contracts.Models
{
    public class Species : ModelBase
    {
        public string SpeciesName { get; set; } = default!;

        // Added this navigation property to represent the collection of breeds for this species
        public ICollection<Breed> Breeds { get; set; } = new List<Breed>();

        public Species() { }
    }
}