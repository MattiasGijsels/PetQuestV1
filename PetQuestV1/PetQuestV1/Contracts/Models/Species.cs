using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Shared;

namespace PetQuestV1.Contracts.Models
{
    public class Species:ModelBase
    {
        public string Name { get; set; } = default!;

        //add csv reader with species??
        public Species() { }
    }
}