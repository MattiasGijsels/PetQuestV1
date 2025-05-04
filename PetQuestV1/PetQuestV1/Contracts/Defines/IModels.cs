
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetQuestV1.Contracts.Defines
{
    public interface IModel
    {
        string Id { get; }
        bool IsDeleted { get; }
    }
}
