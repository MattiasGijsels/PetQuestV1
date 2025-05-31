// Controllers/AdminCrudController.cs
using Microsoft.AspNetCore.Mvc;
using PetQuestV1.Services;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Contracts.Models; // Needed for Species and SpeciesWithBreedCountDto
using PetQuestV1.Contracts.DTOs.Pets; // Needed for PetFormDto
using System.Threading.Tasks;

namespace PetQuestV1.Controllers
{
    [ApiController]
    [Route("api/")]
    public class AdminCrudController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISpeciesService _speciesService;
        private readonly IBreedService _breedService;
        private readonly IPetService _petService;

        public AdminCrudController(
            IUserService userService,
            ISpeciesService speciesService,
            IBreedService breedService,
            IPetService petService)
        {
            _userService = userService;
            _speciesService = speciesService;
            _breedService = breedService;
            _petService = petService;
        }

        // --- USER CRUD (No changes here) ---

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() =>
            Ok(await _userService.GetAllUsersWithPetCountsAsync());

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id) =>
            Ok(await _userService.GetUserByIdAsync(id));

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserFormDto dto)
        {
            if (id != dto.Id) return BadRequest();
            await _userService.UpdateUserAsync(dto);
            return NoContent();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            await _userService.SoftDeleteUserAsync(id);
            return NoContent();
        }

        // --- SPECIES CRUD (Changes applied here) ---

        [HttpGet("species")]
        public async Task<IActionResult> GetSpecies() =>
            // Now calls the service method which returns List<SpeciesWithBreedCountDto>
            Ok(await _speciesService.GetAllSpeciesForAdminAsync());

        [HttpGet("species/{id}")]
        public async Task<IActionResult> GetSpeciesById(string id) =>
            // Now calls the service method which returns SpeciesWithBreedCountDto
            Ok(await _speciesService.GetByIdAsync(id));

        // Keep these as they are, as they accept and update the full Species model (for creation/updates)
        [HttpPost("species")]
        public async Task<IActionResult> AddSpecies([FromBody] Species species)
        {
            await _speciesService.AddAsync(species);
            return CreatedAtAction(nameof(GetSpeciesById), new { id = species.Id }, species);
        }

        [HttpPut("species/{id}")]
        public async Task<IActionResult> UpdateSpecies(string id, [FromBody] Species species)
        {
            if (id != species.Id) return BadRequest();
            await _speciesService.UpdateAsync(species);
            return NoContent();
        }

        [HttpDelete("species/{id}")]
        public async Task<IActionResult> SoftDeleteSpecies(string id)
        {
            await _speciesService.SoftDeleteAsync(id);
            return NoContent();
        }

        // --- BREED CRUD (No changes here, already using DTOs for output) ---

        [HttpGet("breeds")]
        public async Task<IActionResult> GetBreeds() =>
            Ok(await _breedService.GetAllBreedsForAdminAsync());

        [HttpGet("breeds/{id}")]
        public async Task<IActionResult> GetBreedById(string id) =>
            Ok(await _breedService.GetByIdAsync(id));

        [HttpPost("breeds")]
        public async Task<IActionResult> AddBreed([FromBody] Breed breed)
        {
            await _breedService.AddAsync(breed);
            return CreatedAtAction(nameof(GetBreedById), new { id = breed.Id }, breed);
        }

        [HttpPut("breeds/{id}")]
        public async Task<IActionResult> UpdateBreed(string id, [FromBody] Breed breed)
        {
            if (id != breed.Id) return BadRequest();
            await _breedService.UpdateAsync(breed);
            return NoContent();
        }

        [HttpDelete("breeds/{id}")]
        public async Task<IActionResult> SoftDeleteBreed(string id)
        {
            await _breedService.SoftDeleteAsync(id);
            return NoContent();
        }

        // --- PET CRUD (No changes here, already using DTOs for input/output) ---

        [HttpGet("pets")]
        public async Task<IActionResult> GetAllPetsAPI() =>
            Ok(await _petService.GetAllPetsFormDtoAsync());

        [HttpGet("pets/{id}")]
        public async Task<IActionResult> GetPetById(string id)
        {
            var petDto = await _petService.GetPetFormDtoByIdAsync(id);
            if (petDto == null)
            {
                return NotFound();
            }
            return Ok(petDto);
        }

        [HttpPost("pets")]
        public async Task<IActionResult> AddPet([FromBody] PetFormDto petDto)
        {
            await _petService.AddPetAsync(petDto);
            return CreatedAtAction(nameof(GetPetById), new { id = petDto.Id }, petDto);
        }

        [HttpPut("pets/{id}")]
        public async Task<IActionResult> UpdatePet(string id, [FromBody] PetFormDto petDto)
        {
            if (id != petDto.Id) return BadRequest();
            await _petService.UpdatePetAsync(petDto);
            return NoContent();
        }

        [HttpDelete("pets/{id}")]
        public async Task<IActionResult> SoftDeletePet(string id)
        {
            await _petService.SoftDeleteAsync(id);
            return NoContent();
        }
    }
}