using mainykdovanok.Models;
using mainykdovanok.Repositories.Item;
using mainykdovanok.Repositories.User;
using mainykdovanok.Tools;
using mainykdovanok.ViewModels.User;

namespace mainykdovanok.Services
{
    public class ExchangeService
    {
        private DeviceRepo _deviceRepo;
        private UserRepo _userRepo;

        public ExchangeService()
        {
            _deviceRepo = new DeviceRepo();
            _userRepo = new UserRepo();
        }

        public async void NotifyWinner(ExchangeOfferWinnerModel winner, int posterUserId)
        {
            SendEmail emailer = new SendEmail();

            string deviceName = await _deviceRepo.GetDeviceName(winner.DeviceId);
            UserModel user = await _userRepo.GetUser(winner.User);

            await _deviceRepo.SetDeviceWinner(winner.DeviceId, user.Id);

            await emailer.notifyOfferWinner(user.Email, deviceName, winner.DeviceId, winner.DeviceName);

            await _deviceRepo.UpdateDeviceStatus(winner.DeviceId, 2);
            await _deviceRepo.UpdateDeviceStatus(winner.UserDeviceId, 3);
        }
    }
}
