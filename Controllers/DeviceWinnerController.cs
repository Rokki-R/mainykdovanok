using Microsoft.AspNetCore.Mvc;
using mainykdovanok.Repositories.Device;
using mainykdovanok.Repositories.User;
using mainykdovanok.Tools;
using mainykdovanok.ViewModels.Device;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceWinnerController : ControllerBase
    {
        private readonly DeviceRepo _deviceRepo;
        private readonly UserRepo _userRepo;

        public DeviceWinnerController()
        {
            _deviceRepo = new DeviceRepo();
            _userRepo = new UserRepo();
        }

        [HttpPost("confirm")]
        [Authorize]
        public async Task<IActionResult> Confirm(DeviceConfirmViewModel device)
        {

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            //Pridėti tikrinimus, kad neleistų multiple confirmų ir įsitikinti ar tikrai tas useris confirmina
            try
            {
                await _deviceRepo.UpdateDeviceStatus(device.Id, 3);

                await _userRepo.IncrementUserQuantityOfDevicesWon(device.WinnerId);

                await _userRepo.IncrementUserQuantityOfDevicesGifted(device.OwnerId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
