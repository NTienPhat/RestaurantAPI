using API.Data;
using API.Models;
using API.Models.DTO;
using API.Services;
using API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobService _blobService;
        private ApiResponse _response;
        public MenuItemController(ApplicationDbContext db, IBlobService blobService)
        {
            _db = db;
            _blobService = blobService;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = _db.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("{id:int}", Name = "GetMenuItem")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if (id <= 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(x => x.Id == id);
            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }
            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuIteCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuIteCreateDTO.File == null || menuIteCreateDTO.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuIteCreateDTO.File.FileName)}";
                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuIteCreateDTO.Name,
                        Price = menuIteCreateDTO.Price,
                        Category = menuIteCreateDTO.Category,
                        SpecialTag = menuIteCreateDTO.SpecialTag,
                        Description = menuIteCreateDTO.Description,
                        Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuIteCreateDTO.File)
                    };
                    _db.MenuItems.Add(menuItemToCreate);
                    _db.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItemToCreate.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO menuItemUpdateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDTO == null || id != menuItemUpdateDTO.Id)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
                    if (menuItemFromDb == null)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    menuItemFromDb.Name = menuItemUpdateDTO.Name;
                    menuItemFromDb.Price = menuItemUpdateDTO.Price;
                    menuItemFromDb.Category = menuItemUpdateDTO.Category;
                    menuItemFromDb.SpecialTag = menuItemUpdateDTO.SpecialTag;
                    menuItemFromDb.Description = menuItemUpdateDTO.Description;

                    if (menuItemUpdateDTO.File != null && menuItemUpdateDTO.File.Length > 0)
                    {
                        var link = menuItemFromDb.Image.Split('/').Last();
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDTO.File.FileName)}";
                        await _blobService.DeleteBlob(link, SD.SD_Storage_Container);
                        menuItemFromDb.Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemUpdateDTO.File);
                    }

                    _db.MenuItems.Update(menuItemFromDb);
                    _db.SaveChanges();
                    _response.Result = menuItemFromDb;
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _response;
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {

                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return BadRequest();
                }

                MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
                if (menuItemFromDb == null)
                {
                    return BadRequest();
                }

                var link = menuItemFromDb.Image.Split('/').Last();
                await _blobService.DeleteBlob(link, SD.SD_Storage_Container);


                _db.MenuItems.Remove(menuItemFromDb);
                _db.SaveChanges();
                _response.Result = menuItemFromDb;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _response;
        }
    }
}
