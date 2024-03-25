using Microsoft.AspNetCore.Mvc;
using mainykdovanok.Models;
using mainykdovanok.Repositories.Comment;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly CommentRepo _commentRepo;

        public CommentController()
        {
            _commentRepo = new CommentRepo();
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

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

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
