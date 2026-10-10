using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts;
using UserService.Core.Exceptions;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService.Core.Services.UserService _userService;

        public UsersController(UserService.Core.Services.UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserRequest request)
        {
            try
            {
                var user = await _userService.AddUser(request.Name, request.Email);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (DuplicateEmailException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserRequest request)
        {
            try
            {
                var updated = await _userService.UpdateUser(id, request.Name, request.Email);
                return updated ? NoContent() : NotFound();
            }
            catch (DuplicateEmailException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userService.DeleteUser(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
