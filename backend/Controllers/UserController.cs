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
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            var dtos = users.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(ToDto(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] UserDto dto)
        {
            var model = FromDto(dto);
            var created = await _userService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var existing = await _userService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var updated = await _userService.UpdateAsync(FromDto(dto));
            return Ok(ToDto(updated));
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
            return Ok(user.UserRoles.Select(ur => ur.Role.Name).ToList());
        }

        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRoles(int id, [FromBody] List<string> roles)
        {
            var result = await _userService.AssignRolesAsync(id, roles);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(int id, string roleName)
        {
            var result = await _userService.RemoveRoleAsync(id, roleName);
            if (!result) return NotFound();
            return NoContent();
        }

        private static UserDto ToDto(User u) => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Roles = u.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
        };
        private static User FromDto(UserDto dto) => new User
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Email = dto.Email,
            UserRoles = dto.Roles?.Select(r => new UserRole { Role = new Role { Name = r } }).ToList() ?? new List<UserRole>()
        };
    }
}
