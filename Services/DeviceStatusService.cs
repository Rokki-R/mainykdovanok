using mainykdovanok.Repositories.Device;
using mainykdovanok.Repositories.User;
using mainykdovanok.ViewModels.Device;
using mainykdovanok.Tools;

namespace mainykdovanok.Services
{

    public class DeviceStatusService : IHostedService, IDisposable
        {
        private Timer _timer;
        private DeviceRepo _deviceRepo;
        private UserRepo _userRepo;

        public DeviceStatusService()
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
            List<DeviceViewModel> pastEndDateDevices = await _deviceRepo.GetPastEndDateDevices();

            foreach (var device in pastEndDateDevices)
            {
                if (device.Type != "Loterija") // Lottery ads are closed by LotteryService
                {
                    SendEmail emailer = new SendEmail();

                    int posterUserId = device.UserId;
                    string posterUserEmail = await _userRepo.GetUserEmail(posterUserId);

                    // Send email to poster that the ad has expired.
                    await emailer.notifyUserDeviceExpiration(posterUserEmail, device.Name, false);

                    // Update device status to 'Atšauktas'
                    await _deviceRepo.UpdateDeviceStatus(device.Id, 4);
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
