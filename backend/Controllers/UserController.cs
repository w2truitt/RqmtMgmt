using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing users and their role assignments.
    /// Provides endpoints for user CRUD operations and role management functionality.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly RqmtMgmtShared.IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the UserController with the specified user service.
        /// </summary>
        /// <param name="userService">The service for user operations.</param>
        public UserController(RqmtMgmtShared.IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all users from the system including their assigned roles.
        /// </summary>
        /// <returns>A list of all users with their role information.</returns>
        /// <response code="200">Returns the list of users.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by their ID including assigned roles.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user if found, including their role information.</returns>
        /// <response code="200">Returns the requested user.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user in the system with validation for required fields.
        /// </summary>
        /// <param name="dto">The user data to create.</param>
        /// <returns>The created user with its assigned ID.</returns>
        /// <response code="201">Returns the newly created user.</response>
        /// <response code="400">If the user data is invalid or creation fails.</response>
        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] UserDto dto)
        {
            var created = await _userService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing user with new data.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="dto">The updated user data.</param>
        /// <returns>The updated user data.</returns>
        /// <response code="200">Returns the updated user.</response>
        /// <response code="400">If the ID in the URL doesn't match the ID in the request body.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var success = await _userService.UpdateAsync(dto);
            if (!success) return NotFound();
            var updated = await _userService.GetByIdAsync(id);
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the user was successfully deleted.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Retrieves all roles assigned to a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>A list of role names assigned to the user.</returns>
        /// <response code="200">Returns the list of user roles.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpGet("{id}/roles")]
        public async Task<ActionResult<List<string>>> GetRoles(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userService.GetUserRolesAsync(id);
            return Ok(roles);
        }

        /// <summary>
        /// Assigns multiple roles to a user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="roles">The list of role names to assign to the user.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the roles were successfully assigned.</response>
        /// <response code="404">If the user is not found.</response>
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

        /// <summary>
        /// Removes a specific role from a user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="role">The name of the role to remove from the user.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the role was successfully removed.</response>
        /// <response code="404">If the user is not found or doesn't have the specified role.</response>
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