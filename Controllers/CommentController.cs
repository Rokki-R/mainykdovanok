using Microsoft.AspNetCore.Mvc;
using mainykdovanok.Models;
using mainykdovanok.Repositories.Comment;
using mainykdovanok.Repositories.Device;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly CommentRepo _commentRepo;
        private readonly DeviceRepo _deviceRepo;

        public CommentController()
        {
            _commentRepo = new CommentRepo();
            _deviceRepo = new DeviceRepo();
        }

        [HttpGet("getComments/{deviceId}")]
        public async Task<IActionResult> GetComments(int deviceId)
        {
            try
            {
                var result = await _commentRepo.GetAllDeviceComments(deviceId);

                return Ok(new { comments = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("postComment/{deviceId}")]
        public async Task<IActionResult> PostComment(int deviceId, [FromBody] CommentModel comment)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            var device = await _deviceRepo.GetFullById(deviceId);
            if (device.Status != "Aktyvus")
            {
                return Conflict("Negalite pakomentuoti elektronikos prietaiso skelbimo, kuris nebėra aktyvus");
            }

            try
            {
                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

                if (comment.Comment.Length < 10)
                {
                    return BadRequest("Parašytas komentaras yra per trumpas");
                }

                comment.UserId = userId;
                comment.DeviceId = deviceId;
                comment.PostedDateTime = DateTime.Now;

                var result = await _commentRepo.InsertComment(comment);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
