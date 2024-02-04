using mainykdovanok.Repositories.Item;
using mainykdovanok.Repositories.User;
using mainykdovanok.ViewModels.Item;
using mainykdovanok.Tools;

namespace mainykdovanok.Services
{

    public class ItemStatusService : IHostedService, IDisposable
        {
        private Timer _timer;
        private ItemRepo _itemRepo;
        private UserRepo _userRepo;

        public ItemStatusService()
        {
            _itemRepo = new ItemRepo();
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
            List<ItemViewModel> pastEndDateItems = await _itemRepo.GetPastEndDateItems();

            foreach (var item in pastEndDateItems)
            {
                if (item.Type != "Loterija") // Lottery ads are closed by LotteryService
                {
                    SendEmail emailer = new SendEmail();

                    int posterUserId = item.UserId;
                    string posterUserEmail = await _userRepo.GetUserEmail(posterUserId);

                    // Send email to poster that the ad has expired.
                    await emailer.notifyUserItemExpiration(posterUserEmail, item.Name, false);

                    // Update item status to 'Atšauktas'
                    await _itemRepo.UpdateItemStatus(item.Id, 4);
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
