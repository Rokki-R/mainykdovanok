using mainykdovanok.Repositories.Device;
using mainykdovanok.Repositories.User;
using mainykdovanok.Models;
using mainykdovanok.Tools;
using Org.BouncyCastle.Bcpg.Sig;
using mainykdovanok.Models.mainykdovanok.Models.Device;

namespace mainykdovanok.Services
{
    public class MotivationalLetterService
    {
        private DeviceRepo _deviceRepo;
        private UserRepo _userRepo;

        public MotivationalLetterService()
        {
            _deviceRepo = new DeviceRepo();
            _userRepo = new UserRepo();
        }

        public async void NotifyWinner(WinnerSelectionModel winner, int posterUserId)
        {
            SendEmail emailer = new SendEmail();

            string deviceName = await _deviceRepo.GetDeviceName(winner.DeviceId);
            UserModel user = await _userRepo.GetUser(winner.User);

            await _deviceRepo.SetDeviceWinner(winner.DeviceId, user.Id);

            await emailer.notifyLetterWinner(user.Email, deviceName, winner.DeviceId);

            await _deviceRepo.UpdateDeviceStatus(winner.DeviceId, 2);
        }
    }
}
