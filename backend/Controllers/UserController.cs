using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly RqmtMgmtShared.IUserService _userService;
        public UserController(RqmtMgmtShared.IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] UserDto dto)
        {
            var created = await _userService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _userService.UpdateAsync(dto);
            if (!success) return NotFound();
            var updated = await _userService.GetByIdAsync(id);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/roles")]
        public async Task<ActionResult<List<string>>> GetRoles(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userService.GetUserRolesAsync(id);
            return Ok(roles);
        }

        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] List<string> roles)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            foreach (var role in roles)
            {
                await _userService.AssignRoleAsync(id, role);
            }
            return NoContent();
        }

        [HttpDelete("{id}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(int id, string role)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            var userRoles = await _userService.GetUserRolesAsync(id);
            if (!userRoles.Contains(role)) return NotFound();
            await _userService.RemoveRoleAsync(id, role);
            return NoContent();
        }
    }
}