using mainykdovanok.Models;
using mainykdovanok.Models.mainykdovanok.Models.Device;
using mainykdovanok.Repositories.Device;
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

            await _deviceRepo.SetExchangeWinners(winner.DeviceId, user.Id, posterUserId, winner.UserDeviceId);

            await emailer.notifyOfferWinner(user.Email, deviceName, winner.DeviceId, winner.DeviceName);
        }
    }
}
