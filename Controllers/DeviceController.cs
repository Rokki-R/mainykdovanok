using Microsoft.AspNetCore.Mvc;
using mainykdovanok.Models;
using System.Data;
using mainykdovanok.Repositories.Category;
using mainykdovanok.Repositories.Type;
using mainykdovanok.Repositories.Device;
using mainykdovanok.Repositories.Image;
using Microsoft.AspNetCore.Authorization;
using mainykdovanok.ViewModels.Device;
using mainykdovanok.Tools;
using mainykdovanok.Repositories.User;
using Newtonsoft.Json.Linq;
using mainykdovanok.Services;
using static System.Net.Mime.MediaTypeNames;
using mainykdovanok.Repositories.Comment;
using System.Reflection;
using mainykdovanok.Models.mainykdovanok.Models.Device;

namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly TypeRepo _typeRepo;
        private readonly CategoryRepo _categoryRepo;
        private readonly DeviceRepo _deviceRepo;
        private readonly ImageRepo _imageRepo;
        private readonly UserRepo _userRepo;
        private readonly MotivationalLetterService _motivationalLetterService;
        private readonly ExchangeService _exchangeService;
        private readonly QuestionnaireService _questionnaireService;

        public DeviceController()
        {
            _typeRepo = new TypeRepo();
            _categoryRepo = new CategoryRepo();
            _deviceRepo = new DeviceRepo();
            _imageRepo = new ImageRepo();
            _userRepo = new UserRepo();
            _motivationalLetterService = new MotivationalLetterService();
            _exchangeService = new ExchangeService();
            _questionnaireService = new QuestionnaireService();
        }

        [HttpGet("getDevices")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDevices()
        {
            try
            {
                var result = await _deviceRepo.GetAll();

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

        [HttpGet("getDevice/{deviceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDevice(int deviceId)
        {
            try
            {
                var result = await _deviceRepo.GetFullById(deviceId);

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
        [HttpGet("getDeviceOwnerInfo/{deviceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDeviceOwnerInfo(int deviceId)
        {
            try
            {
                var result = await _deviceRepo.GetDeviceOwnerByDeviceId(deviceId);

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

        [HttpGet("getUserDevices")]
        public async Task<IActionResult> GetUserDevices()
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
                var result = await _deviceRepo.GetAllByUser(viewerId);

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

        [HttpGet("getUserWonDevices")]
        public async Task<IActionResult> GetMyWonDevices()
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
                var result = await _deviceRepo.GetUserWonDevices(viewerId);

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
        public async Task<IActionResult> CreateDevice()
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

                if (form.Files.GetFiles("images").Count == 0)
                {
                    return BadRequest("Privaloma įkelti bent vieną nuotrauką!");
                }

                if (string.IsNullOrEmpty(form["name"]) ||
                    string.IsNullOrEmpty(form["description"]) ||
                    string.IsNullOrEmpty(form["location"]) ||
                    string.IsNullOrEmpty(form["category"]) ||
                    string.IsNullOrEmpty(form["type"]))
                {
                    return BadRequest("Privaloma užpildyti visus skelbimo sukūrimo formos laukus!");
                }
                DeviceModel device = new DeviceModel()
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
                };

                string lotteryWinnerDrawDateString = form["lotteryWinnerDrawDate"];
                if (!string.IsNullOrEmpty(lotteryWinnerDrawDateString) && DateTime.TryParse(lotteryWinnerDrawDateString, out DateTime lotteryWinnerDrawDate))
                {
                    device.WinnerDrawDate = lotteryWinnerDrawDate;
                }
                else
                {
                    device.WinnerDrawDate = DateTime.MinValue;
                }

                device.Id = await _deviceRepo.Create(device);

                bool success = await _imageRepo.InsertImages(device);
                if (device.Type == 4)
                {
                    success = await _deviceRepo.InsertQuestions(device);
                }
                return success == true ? Ok(device.Id) : BadRequest();
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
        [HttpGet("getDeviceTypes")]
        public async Task<IActionResult> GetDeviceTypes()
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

        [HttpDelete("delete/{deviceId}")]
        public async Task<IActionResult> Delete(int deviceId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            try
            {
                await _deviceRepo.DeleteDevice(deviceId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getLotteryParticipants/{deviceId}")]
        [Authorize]
        public async Task<IActionResult> GetLotteryParticipants(int deviceId)
        {
            try
            {
                var result = await _deviceRepo.GetLotteryParticipants(deviceId);

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
                var result = await _deviceRepo.LeaveLottery(id, userId);

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
                var device = await _deviceRepo.GetFullById(id);

                if (device == null)
                {
                    return NotFound();
                }

                if (device.Type != "Loterija")
                {
                    return BadRequest("Šis skelbimas yra ne loterijos tipo.");
                }

                // Proceed with entering the lottery
                var result = await _deviceRepo.EnterLottery(id, userId);
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
                var result = await _deviceRepo.IsUserParticipatingInLottery(id, userId);

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
                var searchResults = await _deviceRepo.Search(searchWord);

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search/category/{categoryId}")]
        public async Task<IActionResult> GetDevicesByCategory(int categoryId)
        {
            try
            {
                var result = await _deviceRepo.GetAllByCategory(categoryId);

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
        [HttpGet("getOffers/{deviceId}")]
        [Authorize]
        public async Task<IActionResult> GetOffers(int deviceId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var device = await _deviceRepo.GetFullById(deviceId);
            if (device == null || device.UserId != userId)
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
                var result = await _deviceRepo.GetOffers(deviceId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submitOffer/{deviceId}")]
        public async Task<IActionResult> SubmitOffer(int deviceId)
        {
            var form = await Request.ReadFormAsync();
            ExchangeOfferModel offer = new ExchangeOfferModel()
            {
                SelectedDevice = Convert.ToInt32(form["selectedDevice"]),
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
                var device = await _deviceRepo.GetFullById(deviceId);

                if (device == null)
                {
                    return NotFound();
                }

                // Check if device type is a lottery
                if (device.Type != "Mainai į kita prietaisą")
                {
                    return BadRequest("Šis skelbimas yra ne mainų tipo.");
                }

                var result = await _deviceRepo.SubmitExchangeOffer(deviceId, offer);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submitLetter/{deviceId}")]
        public async Task<IActionResult> SubmitLetter(int deviceId)
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
                var device = await _deviceRepo.GetFullById(deviceId);

                if (device == null)
                {
                    return NotFound();
                }

                if (device.Type != "Motyvacinis laiškas")
                {
                    return BadRequest("Šis skelbimas yra ne motyvacinio laiško tipo.");
                }

                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

                bool hasSubmittedLetter = await _deviceRepo.HasSubmittedLetter(deviceId, userId);
                if (hasSubmittedLetter)
                {
                    return Conflict();
                }

                var form = await Request.ReadFormAsync();
                MotivationalLetterModel letter = new MotivationalLetterModel()
                {
                    Letter = form["letter"].ToString()
                };

                if (letter.Letter.Length < 10)
                {
                    return BadRequest("Motyvacinis laiškas yra per trumpas");
                }

                var result = await _deviceRepo.InsertLetter(deviceId, letter, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getLetters/{deviceId}")]
        [Authorize]
        public async Task<IActionResult> GetLetters(int deviceId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var device = await _deviceRepo.GetFullById(deviceId);
            if (device == null || device.UserId != userId)
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
                var result = await _deviceRepo.GetLetters(deviceId);

                return Ok(new { letters = result });
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
            var device = await _deviceRepo.GetFullById(winner.DeviceId);
            if (device == null || device.UserId != userId)
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

                SendEmail emailer = new SendEmail();

                string deviceName = await _deviceRepo.GetDeviceName(winner.DeviceId);
                UserModel user = await _userRepo.GetUser(winner.User);

                await _deviceRepo.SetExchangeWinners(winner.DeviceId, user.Id, userId, winner.UserDeviceId);

                await emailer.notifyOfferWinner(user.Email, deviceName, winner.DeviceId, winner.DeviceName);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("chooseWinner")]
        public async Task<IActionResult> ChooseWinner([FromBody] WinnerSelectionModel winner)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var device = await _deviceRepo.GetFullById(winner.DeviceId);
            if (device == null || device.UserId != userId)
            {
                return StatusCode(403);
            }

            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {
                if (winner.DeviceType == "Klausimynas")
                {
                    _motivationalLetterService.NotifyWinner(winner, userId);
                }
                else if (winner.DeviceType == "Motyvacinis laiškas")
                {
                    _questionnaireService.NotifyWinner(winner, userId);
                }
                else
                {
                    return BadRequest("Invalid winner selection type.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getQuestionsAndAnswers/{deviceId}")]
        [Authorize]
        public async Task<IActionResult> GetQuestionsAndAnswers(int deviceId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var device = await _deviceRepo.GetFullById(deviceId);
            if (device == null || device.UserId != userId)
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
                var result = await _deviceRepo.GetQuestionsAndAnswers(deviceId);

                return Ok(new { questionnaires = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getQuestions/{deviceId}")]
        [Authorize]
        public async Task<IActionResult> GetQuestions(int deviceId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _deviceRepo.GetQuestions(deviceId);

                return Ok(new { questionnaires = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("submitAnswers/{deviceId}")]
        public async Task<IActionResult> SubmitAnswers(int deviceId, [FromBody] List<AnswerModel> answers)
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

            var device = await _deviceRepo.GetFullById(deviceId);

            if (device == null)
            {
                return NotFound();
            }

            if (device.Type != "Klausimynas")
            {
                return BadRequest("Šis skelbimas yra ne klausimyno tipo.");
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            bool hasSubmittedLetter = await _deviceRepo.HasSubmittedAnswers(deviceId, userId);
            if (hasSubmittedLetter)
            {
                return Conflict();
            }

            try
            {
                var result = await _deviceRepo.InsertAnswers(deviceId, answers, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDevice(int id, IFormCollection form)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
            var device = await _deviceRepo.GetFullById(id);
            if (device == null || device.UserId != userId)
            {
                return StatusCode(403);
            }

            if (!User.IsInRole("0"))
            {
                return StatusCode(403);
            }

            try
            {

                if (string.IsNullOrEmpty(form["name"]) ||
                    string.IsNullOrEmpty(form["description"]) ||
                    string.IsNullOrEmpty(form["location"]) ||
                    string.IsNullOrEmpty(form["category"]))
                {
                    return BadRequest("Privaloma užpildyti visus skelbimo sukūrimo formos laukus!");
                }

                DeviceModel updateDevice = new DeviceModel()
                {
                    Id = id,
                    Name = form["name"],
                    Description = form["description"],
                    Category = Convert.ToInt32(form["fk_Category"]),
                    Images = Request.Form.Files.GetFiles("images").ToList(),
                };
              
                var imagesToDelete = form["imagesToDelete"].Select(idStr => Convert.ToInt32(idStr)).ToList();

                if (imagesToDelete.Count > 0)
                {
                    await _imageRepo.Delete(imagesToDelete);
                }

                await _deviceRepo.Update(updateDevice);

                await _imageRepo.InsertImages(updateDevice);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
