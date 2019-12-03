using System.Threading.Tasks;
using DatingApp.API.Data.Repositories;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IValueRepository _repo;

        public ValuesController(IValueRepository repo)
        {
            this._repo = repo;
        }

        // GET api/values
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetValues()
        {
            return Ok(await _repo.findAll());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetValue(int id)
        {
            return Ok(await _repo.findById(id));
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post(ValueForPostDto valueForPostDto)
        {
            var valueToCreate = new Value() {
                Name = valueForPostDto.Name
            };
            
            await _repo.create(valueToCreate);
            return Created("api/values/", valueToCreate);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ValueForPutDto valueForPutDto)
        {
            var valueToUpdate = new Value() {
                Id = id,
                Name = valueForPutDto.Name
            };

            await _repo.updateById(valueToUpdate);
            return Ok();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.deleteById(id);
            return Ok();
        }
    }
}