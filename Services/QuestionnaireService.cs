using mainykdovanok.Repositories.Item;
using mainykdovanok.Repositories.User;
using mainykdovanok.ViewModels.User;
using mainykdovanok.Models;
using mainykdovanok.Repositories.Item;
using mainykdovanok.Repositories.User;
using mainykdovanok.Tools;
using mainykdovanok.ViewModels.User;

namespace mainykdovanok.Services
{
    public class QuestionnaireService
    {
        private DeviceRepo _deviceRepo;
        private UserRepo _userRepo;

        public QuestionnaireService()
        {
            _deviceRepo = new DeviceRepo();
            _userRepo = new UserRepo();
        }

        public async void NotifyWinner(QuestionnaireWinnerModel winner, int posterUserId)
        {
            SendEmail emailer = new SendEmail();

            string deviceName = await _deviceRepo.GetDeviceName(winner.DeviceId);
            UserModel user = await _userRepo.GetUser(winner.User);

            await _deviceRepo.SetDeviceWinner(winner.DeviceId, user.Id);

            await emailer.notifyQuestionnaireWinner(user.Email, deviceName, winner.DeviceId);

            await _deviceRepo.UpdateDeviceStatus(winner.DeviceId, 2);
        }
    }
}
