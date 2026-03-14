using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CleanArchitectureDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints require a valid JWT by default
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ── GET /api/users ────────────────────────────────────────────────────
        /// <summary>Get all active users. Admin only.</summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // ── GET /api/users/me ─────────────────────────────────────────────────
        /// <summary>Get the currently authenticated user's profile.</summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe()
        {
            var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(rawId, out var userId))
                return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            return Ok(user);
        }

        // ── GET /api/users/{id} ───────────────────────────────────────────────
        /// <summary>Get a user by ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        // ── PUT /api/users/{id} ───────────────────────────────────────────────
        /// <summary>Update first name, last name, and phone number.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            // Ensure route id matches body id
            if (id != dto.Id)
                return BadRequest(new { detail = "Route id does not match body id." });

            await _userService.UpdateAsync(dto);
            return NoContent();
        }

        // ── DELETE /api/users/{id} ────────────────────────────────────────────
        /// <summary>Hard-delete a user. Admin only.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
    }
}
