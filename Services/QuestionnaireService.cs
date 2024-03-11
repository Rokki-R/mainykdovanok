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
        private ItemRepo _itemRepo;
        private UserRepo _userRepo;

        public QuestionnaireService()
        {
            _itemRepo = new ItemRepo();
            _userRepo = new UserRepo();
        }

        public async void NotifyWinner(QuestionnaireWinnerModel winner, int posterUserId)
        {
            SendEmail emailer = new SendEmail();

            string itemName = await _itemRepo.GetItemName(winner.ItemId);
            UserModel user = await _userRepo.GetUser(winner.User);

            await _itemRepo.SetItemWinner(winner.ItemId, user.Id);

            await emailer.notifyQuestionnaireWinner(user.Email, itemName, winner.ItemId);

            await _itemRepo.UpdateItemStatus(winner.ItemId, 2);
        }
    }
}
