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

        public ItemController()
        {
            _typeRepo = new TypeRepo();
            _categoryRepo = new CategoryRepo();
            _itemRepo = new ItemRepo();
            _imageRepo = new ImageRepo();
            _userRepo = new UserRepo();
        }

        [HttpGet("getItems")]
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

        [HttpGet("getUserItems")]
        public async Task<IActionResult> GetUserItems()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
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
                if (item.Type == 2)
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

    }
}
