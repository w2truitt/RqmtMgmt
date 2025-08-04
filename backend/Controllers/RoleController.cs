using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _service;
        public RoleController(IRoleService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpPost]
        public async Task<ActionResult<Role>> Create([FromBody] string name)
        {
            var role = await _service.CreateAsync(name);
            return CreatedAtAction(nameof(GetAll), new { id = role.Id }, role);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
