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
        public async Task<ActionResult<List<string>>> GetAllRoles()
        {
            var roles = await _service.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            await _service.CreateRoleAsync(roleName);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRole([FromQuery] string roleName)
        {
            await _service.DeleteRoleAsync(roleName);
            return NoContent();
        }
    }
}
