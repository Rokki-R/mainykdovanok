using mainykdovanok.Repositories.Device;
using mainykdovanok.Repositories.User;
using mainykdovanok.Tools;
using mainykdovanok.ViewModels.Device;

namespace mainykdovanok.Services
{
    public class LotteryService : IHostedService, IDisposable
    {
        private Timer _timer;
        private DeviceRepo _deviceRepo;
        private UserRepo _userRepo;

        public LotteryService()
        {
            _deviceRepo = new DeviceRepo();
            _userRepo = new UserRepo();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoTask, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        private async void DoTask(object? state)
        {
            List<DeviceLotteryViewModel> dueLotteriesList = await _deviceRepo.GetDueLotteries();

            foreach (var lottery in dueLotteriesList)
            {
                SendEmail emailer = new SendEmail();

                int posterUserId = lottery.UserId;
                string posterUserEmail = await _userRepo.GetUserEmail(posterUserId);

                if (lottery.Participants > 0)
                {
                    int winnerUserId = await _deviceRepo.DrawLotteryWinner(lottery.Id);
                    string winnerUserEmail = await _userRepo.GetUserEmail(winnerUserId);

                    await _deviceRepo.SetDeviceWinner(lottery.Id, winnerUserId);

                    // Send email notifications to poster and winner

                    await emailer.notifyLotteryPosterWin(posterUserEmail, lottery.Name, winnerUserEmail);
                    await emailer.notifyLotteryWinner(winnerUserEmail, lottery.Name, lottery.Id);

                    // Update device status to 'Ištrinktas laimėtojas'
                    await _deviceRepo.UpdateDeviceStatus(lottery.Id, 2);

                }
                else
                {
                    await emailer.notifyUserDeviceAboutNoParticipants(posterUserEmail, lottery.Name, true);

                    // Update device status to 'Atšauktas'
                    await _deviceRepo.DeleteDevice(lottery.Id);
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
