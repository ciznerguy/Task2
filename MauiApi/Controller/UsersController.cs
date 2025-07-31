using MauiApi.DBAccess;
using MauiModels.Models;

using Microsoft.AspNetCore.Mvc;

namespace MauiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDataAccess _data = new();

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await _data.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _data.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] UserLoginRequest login)
        {
            // ✅ שימוש ב־Username במקום Email
            var user = await _data.GetUserByCredentialsAsync(login.Username, login.Password);

            if (user is null)
                return Unauthorized("שם משתמש או סיסמה שגויים");

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] User user)
        {
            await _data.AddUserAsync(user);
            return Ok();
        }
    }
}
