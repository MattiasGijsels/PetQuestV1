using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Services
{
    public class PetService : IPetService
    {
        private readonly List<Pet> _pets = new();

        public Task<List<Pet>> GetAllAsync() => Task.FromResult(_pets);

        public Task<Pet> GetByIdAsync(string id) =>
            Task.FromResult(_pets.FirstOrDefault(p => p.Id == id));

        public Task AddAsync(Pet pet)
        {
            pet.Id = Guid.NewGuid().ToString();
            _pets.Add(pet);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Pet pet)
        {
            var idx = _pets.FindIndex(p => p.Id == pet.Id);
            if (idx >= 0) _pets[idx] = pet;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            _pets.RemoveAll(p => p.Id == id);
            return Task.CompletedTask;
        }
    }
}