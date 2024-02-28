using mainykdovanok.Repositories.Item;
using mainykdovanok.Repositories.User;
using mainykdovanok.Models;
using mainykdovanok.Tools;
using Org.BouncyCastle.Bcpg.Sig;

namespace mainykdovanok.Services
{
    public class MotivationalLetterService
    {
        private ItemRepo _itemRepo;
        private UserRepo _userRepo;

        public MotivationalLetterService()
        {
            _itemRepo = new ItemRepo();
            _userRepo = new UserRepo();
        }

        public async void NotifyWinner(MotivationalLetterWinnerModel winner, int posterUserId)
        {
            SendEmail emailer = new SendEmail();

            string itemName = await _itemRepo.GetItemName(winner.ItemId);
            UserModel user = await _userRepo.GetUser(winner.User);

            await _itemRepo.SetItemWinner(winner.ItemId, user.Id);

            await emailer.notifyLetterWinner(user.Email, itemName, winner.ItemId);

            await _itemRepo.UpdateItemStatus(winner.ItemId, 2);

            await _userRepo.IncrementUserQuantityOfItemsGifted(posterUserId);
            await _userRepo.IncrementUserQuantityOfItemsWon(user.Id);
        }
    }
}
