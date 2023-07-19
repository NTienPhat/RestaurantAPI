using API.Data;
using API.Models;
using API.Models.DTO;
using API.Services;
using API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
            _response = new ApiResponse();
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
        {
            try
            {
                var order = _db.OrderHeaders.Include(u => u.OrderDetails)
                .ThenInclude(u => u.MenuItem)
                .OrderByDescending(u => u.OrderHeaderId);
                if (!string.IsNullOrEmpty(userId))
                {
                    _response.Result = order.Where(u => u.ApplicationUserId == userId);
                }
                else
                {
                    _response.Result = order;
                }
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>>GetById(int id)
        {
            if(id <=0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            var order = _db.OrderHeaders.Include(x => x.OrderDetails).ThenInclude(x => x.MenuItem)

                .FirstOrDefault(x => x.OrderHeaderId == id);
            if(order == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = order;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeader)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeader.ApplicationUserId,
                    PickupEmail = orderHeader.PickupEmail,
                    PickupName = orderHeader.PickupName,
                    PickupPhoneNumber = orderHeader.PickupPhoneNumber,
                    OrderTotal = orderHeader.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentId = orderHeader.StripePaymentIntentId,
                    TotalItems = orderHeader.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeader.Status)? SD.status_pending : orderHeader.Status
                };
                if(ModelState.IsValid)
                {
                    _db.OrderHeaders.Add(order);
                    _db.SaveChanges();
                    foreach(var orderDetails in orderHeader.OrderDetailsDTO)
                    {
                        OrderDetails details = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetails.ItemName,
                            MenuItemId = orderDetails.MenuItemId,
                            Price = orderDetails.Price,
                            Quantity = orderDetails.Quantity,
                        };
                        _db.OrderDetails.Add(details);
                    }
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> updateOrderHeader(int id, [FromBody]OrderHeaderUpdateDTO orderHeaderUpdateDTO)
        {
            try
            {
                if(orderHeaderUpdateDTO == null || orderHeaderUpdateDTO.OrderHeaderId != id)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }
                OrderHeader order = _db.OrderHeaders.FirstOrDefault(x => x.OrderHeaderId == id);
                if(order == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }
                if(!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupName))
                {
                    order.PickupName = orderHeaderUpdateDTO.PickupName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupEmail))
                {
                    order.PickupEmail = orderHeaderUpdateDTO.PickupEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupPhoneNumber))
                {
                    order.PickupPhoneNumber = orderHeaderUpdateDTO.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.Status))
                {
                    order.Status = orderHeaderUpdateDTO.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.StripePaymentIntentId))
                {
                    order.StripePaymentIntentId = orderHeaderUpdateDTO.StripePaymentIntentId;
                }
                _db.SaveChanges();
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
