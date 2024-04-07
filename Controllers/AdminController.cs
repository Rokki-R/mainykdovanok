using mainykdovanok.Repositories.User;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserRepo _userRepo;

        public AdminController()
        {
            _userRepo = new UserRepo();
        }
        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!User.IsInRole("1"))
            {
                return StatusCode(403);
            }

            try
            {
                var result = await _userRepo.GetUsers();
                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("updateUserStatus/{userId}")]
        public async Task<IActionResult> updateUserStatus(int userId, string action)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!User.IsInRole("1"))
            {
                return StatusCode(403);
            }

            try
            {
                int status = 0;
                if (action == "Blokuoti")
                {
                    status = 2;
                }
                else
                {
                    status = 1;
                }
                var result = await _userRepo.UpdateUserStatus(userId, status);
                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
