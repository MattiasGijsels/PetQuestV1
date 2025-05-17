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
        public string SpeciesId { get; set; } // FK to Species

        [ForeignKey("SpeciesId")]
        public Species Species { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string Breed { get; set; } = default!;

        public int Advantage { get; set; } = 5;
        //number that defines how much advantage a pet has when it enters a minigame,
        //it's the result of Virtual pet score game.

        public int Age { get; set; }

        // Foreign Key to the User
        [Required]
        public string OwnerId { get; set; }

        // Navigation Property to the User
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; } // One to many relation ship, one owner can have many pets

        public Pet() : base() { }
    }
}