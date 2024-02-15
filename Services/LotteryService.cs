using mainykdovanok.Repositories.Item;
using mainykdovanok.Repositories.User;
using mainykdovanok.Tools;
using mainykdovanok.ViewModels.Item;

namespace mainykdovanok.Services
{
    public class LotteryService : IHostedService, IDisposable
    {
        private Timer _timer;
        private ItemRepo _itemRepo;
        private UserRepo _userRepo;

        public LotteryService()
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
            List<ItemLotteryViewModel> dueLotteriesList = await _itemRepo.GetDueLotteries();

            foreach (var lottery in dueLotteriesList)
            {
                SendEmail emailer = new SendEmail();

                int posterUserId = lottery.UserId;
                string posterUserEmail = await _userRepo.GetUserEmail(posterUserId);

                if (lottery.Participants > 0)
                {
                    int winnerUserId = await _itemRepo.DrawLotteryWinner(lottery.Id);
                    string winnerUserEmail = await _userRepo.GetUserEmail(winnerUserId);

                    await _itemRepo.SetItemWinner(lottery.Id, winnerUserId);

                    // Send email notifications to poster and winner

                    await emailer.notifyLotteryPosterWin(posterUserEmail, lottery.Name, winnerUserEmail);
                    await emailer.notifyLotteryWinner(winnerUserEmail, lottery.Name, lottery.Id);

                    // Update item status to 'Ištrinktas laimėtojas'
                    await _itemRepo.UpdateItemStatus(lottery.Id, 2);

                    await _userRepo.IncrementUserQuantityOfItemsGifted(posterUserId);
                    await _userRepo.IncrementUserQuantityOfItemsWon(winnerUserId);
                }
                else
                {
                    // Send email notification to poster that the item ad has expired due to not enough participants.
                    await emailer.notifyUserItemExpiration(posterUserEmail, lottery.Name, true);

                    // Update item status to 'Atšauktas'
                    await _itemRepo.UpdateItemStatus(lottery.Id, 4);
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
