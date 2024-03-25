using mainykdovanok.Repositories.Device;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly DeviceRepo _deviceRepo;

        public HomeController()
        {
            _deviceRepo = new DeviceRepo();
        }

        [HttpGet("getDevices")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDevices()
        {
            try
            {
                var result = await _deviceRepo.GetAll();

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
