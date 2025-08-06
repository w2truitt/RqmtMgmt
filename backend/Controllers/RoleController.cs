using RqmtMgmtShared;
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
        private readonly RqmtMgmtShared.IRoleService _service;
        public RoleController(RqmtMgmtShared.IRoleService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
        {
            var roles = await _service.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] string roleName)
        {
            var created = await _service.CreateRoleAsync(roleName);
            if (created == null) return BadRequest();
            return Ok(created);
        }

        [HttpPost("dto")]
        public async Task<ActionResult<RoleDto>> CreateRoleFromDto([FromBody] RoleDto roleDto)
        {
            var created = await _service.CreateRoleAsync(roleDto.Name);
            if (created == null) return BadRequest();
            return Ok(created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var deleted = await _service.DeleteRoleAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}