using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for managing user roles with creation, retrieval, and deletion operations.
    /// Provides endpoints for role management with validation and duplicate prevention.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly RqmtMgmtShared.IRoleService _service;

        /// <summary>
        /// Initializes a new instance of the RoleController with the specified role service.
        /// </summary>
        /// <param name="service">The service for role operations.</param>
        public RoleController(RqmtMgmtShared.IRoleService service) => _service = service;

        /// <summary>
        /// Retrieves all roles from the system.
        /// </summary>
        /// <returns>A list of all available roles.</returns>
        /// <response code="200">Returns the list of roles.</response>
        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
        {
            var roles = await _service.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Creates a new role with the specified name.
        /// Performs case-insensitive duplicate checking and returns existing role if found.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <returns>The created or existing role.</returns>
        /// <response code="200">Returns the created or existing role.</response>
        /// <response code="400">If the role name is invalid or creation fails.</response>
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] string roleName)
        {
            var created = await _service.CreateRoleAsync(roleName);
            if (created == null) return BadRequest();
            return Ok(created);
        }

        /// <summary>
        /// Creates a new role from a RoleDto object.
        /// Alternative endpoint that accepts a full DTO instead of just the role name.
        /// </summary>
        /// <param name="roleDto">The role DTO containing the role name.</param>
        /// <returns>The created or existing role.</returns>
        /// <response code="200">Returns the created or existing role.</response>
        /// <response code="400">If the role data is invalid or creation fails.</response>
        [HttpPost("dto")]
        public async Task<ActionResult<RoleDto>> CreateRoleFromDto([FromBody] RoleDto roleDto)
        {
            var created = await _service.CreateRoleAsync(roleDto.Name);
            if (created == null) return BadRequest();
            return Ok(created);
        }

        /// <summary>
        /// Deletes a role by its ID if it's not assigned to any users.
        /// Prevents deletion of roles that are currently in use to maintain data integrity.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the role was successfully deleted.</response>
        /// <response code="404">If the role is not found or cannot be deleted because it's in use.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var deleted = await _service.DeleteRoleAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}