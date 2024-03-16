using Microsoft.AspNetCore.Mvc;
using mainykdovanok.Models;
using System.Data;
using mainykdovanok.Repositories.Category;
using mainykdovanok.Repositories.Type;
using mainykdovanok.Repositories.Item;
using mainykdovanok.Repositories.Image;
using Microsoft.AspNetCore.Authorization;
using mainykdovanok.ViewModels.Item;
using mainykdovanok.Tools;
using mainykdovanok.Repositories.User;
using Newtonsoft.Json.Linq;
using mainykdovanok.Services;
using static System.Net.Mime.MediaTypeNames;
using mainykdovanok.Repositories.Comment;
using System.Reflection;

namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly TypeRepo _typeRepo;
        private readonly CategoryRepo _categoryRepo;
        private readonly ItemRepo _itemRepo;
        private readonly ImageRepo _imageRepo;
        private readonly UserRepo _userRepo;
        private readonly CommentRepo _commentRepo;
        private readonly MotivationalLetterService _motivationalLetterService;
        private readonly ExchangeService _exchangeService;
        private readonly QuestionnaireService _questionnaireService;

        public ItemController()
        {
            _typeRepo = new TypeRepo();
            _categoryRepo = new CategoryRepo();
            _itemRepo = new ItemRepo();
            _imageRepo = new ImageRepo();
            _userRepo = new UserRepo();
            _commentRepo = new CommentRepo();
            _motivationalLetterService = new MotivationalLetterService();
            _exchangeService = new ExchangeService();
            _questionnaireService = new QuestionnaireService();
        }

        [HttpGet("getItems")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItems()
        {
            try
            {
                var result = await _itemRepo.GetAll();

                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getItem/{itemId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItem(int itemId)
        {
            try
            {
                var result = await _itemRepo.GetFullById(itemId);

                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getItemOwnerInfo/{itemId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItemOwnerInfo(int itemId)
        {
            try
            {
                var result = await _itemRepo.GetItemOwnerByItemId(itemId);

                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getUserItems")]
        public async Task<IActionResult> GetUserItems()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                int viewerId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
                var result = await _itemRepo.GetAllByUser(viewerId);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateItem()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                var form = await Request.ReadFormAsync();
                ItemModel item = new ItemModel()
                {
                    Name = form["name"].ToString(),
                    Description = form["description"].ToString(),
                    Location = form["location"].ToString(),
                    Status = 1,
                    User = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value),
                    Category = Convert.ToInt32(form["category"]),
                    Type = Convert.ToInt32(form["type"]),
                    Images = form.Files.GetFiles("images").ToList(),
                    Questions = form["questions"].ToList(),
                    EndDate = Convert.ToDateTime(form["endDate"]),
                };

                item.Id = await _itemRepo.Create(item);

                bool success = await _imageRepo.InsertImages(item);
                if (item.Type == 4)
                {
                    success = await _itemRepo.InsertQuestions(item);
                }
                return success == true ? Ok(item.Id) : BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }
        [HttpGet("getCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryRepo.GetAll();

                if (categories == null)
                {
                    return BadRequest();
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getItemTypes")]
        public async Task<IActionResult> GetItemTypes()
        {
            try
            {
                var types = await _typeRepo.GetAll();

                if (types == null)
                {
                    return BadRequest();
                }

                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete/{itemId}")]
        public async Task<IActionResult> Delete(int itemId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            try
            {
                await _itemRepo.DeleteItem(itemId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getLotteryParticipants/{itemId}")]
        [Authorize]
        public async Task<IActionResult> GetLotteryParticipants(int itemId)
        {
            try
            {
                var result = await _itemRepo.GetLotteryParticipants(itemId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leaveLottery/{id}")]
        [Authorize]
        public async Task<IActionResult> LeaveLottery(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            try
            {
                var result = await _itemRepo.LeaveLottery(id, userId);

                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("enterLottery/{id}")]
        public async Task<IActionResult> EnterLottery(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            try
            {
                var item = await _itemRepo.GetFullById(id);

                if (item == null)
                {
                    return NotFound();
                }

                // Check if item type is a lottery
                if (item.Type != "Loterija")
                {
                    return BadRequest("Šis skelbimas yra ne loterijos tipo.");
                }

                // Proceed with entering the lottery
                var result = await _itemRepo.EnterLottery(id, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("isUserParticipatingInLottery/{id}")]
        [Authorize]
        public async Task<IActionResult> IsUserParticipatingInLottery(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            try
            {
                var result = await _itemRepo.IsUserParticipatingInLottery(id, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("submitWinnerDetails")]
        [Authorize]
        public async Task<IActionResult> SubmitWinnerDetails(ItemWinnerViewModel itemWinnerDetails)
        {

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                // Send an email to the item poster with winner details.
                SendEmail emailer = new SendEmail();
                bool result = await emailer.sendWinnerDetails(itemWinnerDetails.PosterEmail, itemWinnerDetails.ItemName, itemWinnerDetails.Phone, itemWinnerDetails.Message);

                // Set item status to 'Užbaigtas'
                await _itemRepo.UpdateItemStatus(itemWinnerDetails.ItemId, 3);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchWord)
        {
            try
            {
                var searchResults = await _itemRepo.Search(searchWord);

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search/category/{categoryId}")]
        public async Task<IActionResult> GetItemsByCategory(int categoryId)
        {
            try
            {
                var result = await _itemRepo.GetAllByCategory(categoryId);

                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getOffers/{itemId}")]
        [Authorize]
        public async Task<IActionResult> GetOffers(int itemId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var item = await _itemRepo.GetFullById(itemId);
            if (item == null || item.UserId != userId)
            {
                return StatusCode(403);
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }
            try
            {
                var result = await _itemRepo.GetOffers(itemId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submitOffer/{itemId}")]
        public async Task<IActionResult> SubmitOffer(int itemId)
        {
            var form = await Request.ReadFormAsync();
            ExchangeOfferModel offer = new ExchangeOfferModel()
            {
                SelectedItem = Convert.ToInt32(form["selectedItem"]),
                Message = form["message"].ToString(),
            };

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                var item = await _itemRepo.GetFullById(itemId);

                if (item == null)
                {
                    return NotFound();
                }

                // Check if item type is a lottery
                if (item.Type != "Mainai į kita prietaisą")
                {
                    return BadRequest("Šis skelbimas yra ne mainų tipo.");
                }

                var result = await _itemRepo.SubmitExchangeOffer(itemId, offer);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("chooseExchangeOfferWinner")]
        public async Task<IActionResult> ChooseExchangeOfferWinner([FromBody] ExchangeOfferWinnerModel winner)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var item = await _itemRepo.GetFullById(winner.ItemId);
            if (item == null || item.UserId != userId)
            {
                return StatusCode(403);
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }
            try
            {
                _exchangeService.NotifyWinner(winner, userId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submitLetter/{itemId}")]
        public async Task<IActionResult> SubmitLetter(int itemId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                var item = await _itemRepo.GetFullById(itemId);

                if (item == null)
                {
                    return NotFound();
                }

                // Check if item type is a motivational letter
                if (item.Type != "Motyvacinis laiškas")
                {
                    return BadRequest("Šis skelbimas yra ne motyvacinio laiško tipo.");
                }

                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

                bool hasSubmittedLetter = await _itemRepo.HasSubmittedLetter(itemId, userId);
                if (hasSubmittedLetter)
                {
                    return Conflict();
                }

                var form = await Request.ReadFormAsync();
                MotivationalLetterModel letter = new MotivationalLetterModel()
                {
                    Letter = form["letter"].ToString()
                };

                var result = await _itemRepo.InsertLetter(itemId, letter, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getLetters/{itemId}")]
        [Authorize]
        public async Task<IActionResult> GetLetters(int itemId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var item = await _itemRepo.GetFullById(itemId);
            if (item == null || item.UserId != userId)
            {
                return StatusCode(403);
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                var result = await _itemRepo.GetLetters(itemId);

                return Ok(new { letters = result });
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("chooseLetterWinner")]
        public async Task<IActionResult> ChooseLetterWinner([FromBody] MotivationalLetterWinnerModel winner)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var item = await _itemRepo.GetFullById(winner.ItemId);
            if (item == null || item.UserId != userId)
            {
                return StatusCode(403);
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                _motivationalLetterService.NotifyWinner(winner, userId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getComments/{itemId}")]
        public async Task<IActionResult> GetComments(int itemId)
        {
            try
            {
                var result = await _commentRepo.GetAllItemComments(itemId);

                return Ok(new { comments = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("postComment/{itemId}")]
        public async Task<IActionResult> PostComment(int itemId, [FromBody] CommentModel comment)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

                comment.UserId = userId;
                comment.ItemId = itemId;
                comment.PostedDateTime = DateTime.Now;

                var result = await _commentRepo.InsertComment(comment);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getQuestionsAndAnswers/{itemId}")]
        [Authorize]
        public async Task<IActionResult> GetQuestionsAndAnswers(int itemId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var item = await _itemRepo.GetFullById(itemId);
            if (item == null || item.UserId != userId)
            {
                return StatusCode(403);
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                var result = await _itemRepo.GetQuestionsAndAnswers(itemId);

                return Ok(new { questionnaires = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getQuestions/{itemId}")]
        [Authorize]
        public async Task<IActionResult> GetQuestions(int itemId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _itemRepo.GetQuestions(itemId);

                return Ok(new { questionnaires = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submitAnswers/{itemId}")]
        public async Task<IActionResult> SubmitAnswers(int itemId, [FromBody] List<AnswerModel> answers)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            //Patikrinti ar prisijungęs naudotojas nėra admin
            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            var item = await _itemRepo.GetFullById(itemId);

            if (item == null)
            {
                return NotFound();
            }

            // Check if item type is a questionnaire
            if (item.Type != "Klausimynas")
            {
                return BadRequest("Šis skelbimas yra ne klausimyno tipo.");
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            bool hasSubmittedLetter = await _itemRepo.HasSubmittedAnswers(itemId, userId);
            if (hasSubmittedLetter)
            {
                return Conflict();
            }

            try
            {
                var result = await _itemRepo.InsertAnswers(itemId, answers, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("chooseQuestionnaireWinner")]
        public async Task<IActionResult> ChooseQuestionnaireWinner([FromBody] QuestionnaireWinnerModel winner)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var item = await _itemRepo.GetFullById(winner.ItemId);
            if (item == null || item.UserId != userId)
            {
                return StatusCode(403);
            }

            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }
            try
            {
                _questionnaireService.NotifyWinner(winner, userId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateItem(int id, IFormCollection form)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var item = await _itemRepo.GetFullById(id);
            if (item == null || item.UserId != userId)
            {
                return StatusCode(403);
            }

            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {   
                ItemModel updateItem = new ItemModel()
                {
                    Id = id,
                    Name = form["name"],
                    Description = form["description"],
                    Category = Convert.ToInt32(form["fk_Category"]),
                    Images = Request.Form.Files.GetFiles("images").ToList(),
                    Type = Convert.ToInt32(form["type"]),
                    Questions = form["questions"].ToList()
                };
                var updateItemType = await _typeRepo.GetType(updateItem.Type);

                var answers = await _itemRepo.GetAnswers(id);
                if (item.Type != updateItemType.Name)
                {
                    switch (item.Type)
                    {
                        case "Motyvacinis laiškas":
                            var letters = await _itemRepo.GetLetters(id);
                            if (letters.Any())
                            {
                                return BadRequest("Negalite pakeisti skelbimo tipo, nes jūsų skelbimas jau sulaukė susidomėjimo!");
                            }
                            break;
                        case "Mainai į kita prietaisą":
                            var offers = await _itemRepo.GetOffers(id);
                            if (offers.Any())
                            {
                                return BadRequest("Negalite pakeisti skelbimo tipo, nes jūsų skelbimas jau sulaukė susidomėjimo!");
                            }
                            break;
                        case "Loterija":
                            var participants = await _itemRepo.GetLotteryParticipants(id);
                            if (participants.Any())
                            {
                                return BadRequest("Negalite pakeisti skelbimo tipo, nes jūsų skelbimas jau sulaukė susidomėjimo!");
                            }
                            break;
                        case "Klausimynas":
                            if (answers.Any())
                            {
                                return BadRequest("Negalite pakeisti skelbimo tipo, nes jūsų skelbimas jau sulaukė susidomėjimo!");
                            }
                            break;
                        default:
                            break;
                    }
                }

                //If updated item type is 'Klausimynas'
                if (updateItem.Type == 4)
                {
                    var itemQuestions = await _itemRepo.GetQuestions(id);
                    List<string> itemQuestionStrings = itemQuestions.Select(question => question.Question).ToList();

                    bool questionsEdited = !Enumerable.SequenceEqual<string>(updateItem.Questions, itemQuestionStrings);

                    if (questionsEdited)
                    {
                        if (!answers.Any())
                        {
                            await _itemRepo.DeleteQuestions(id);
                            await _itemRepo.InsertQuestions(updateItem);
                        }
                        else
                        {
                            return BadRequest("Negalite pakeisti skelbimo klausimų, nes į dabartinius klausimus jau atsakė bent 1 asmuo!");
                        }
                    }
                }

                //If item type before update was klausimynas and updated item type is different
                if (item.Type == "Klausimynas" && updateItemType.Name != "Klausimynas")
                {
                    await _itemRepo.DeleteQuestions(id);
                }
                var imagesToDelete = form["imagesToDelete"].Select(idStr => Convert.ToInt32(idStr)).ToList();

                if (imagesToDelete.Count > 0)
                {
                    await _imageRepo.Delete(imagesToDelete);
                }

                await _itemRepo.Update(updateItem);

                await _imageRepo.InsertImages(updateItem);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
