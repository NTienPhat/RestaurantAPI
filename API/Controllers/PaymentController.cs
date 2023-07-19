using API.Data;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private ApiResponse _response;
        public PaymentController(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _response = new ApiResponse();
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId)
        {
            ShoppingCart shoppingCart = _db.ShoppingCarts.Include(x => x.CartItems).ThenInclude(x => x.MenuItem)
                .FirstOrDefault(x => x.UserId == userId);
            if(shoppingCart == null || shoppingCart.CartItems == null || shoppingCart.CartItems.Count == 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            #region Create Payment Intent

            //StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";

            //var options = new PaymentIntentCreateOptions
            //{
            //    Amount = 2000,
            //    Currency = "usd",
            //    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            //    {
            //        Enabled = true,
            //    },
            //};
            //var service = new PaymentIntentService();
            //service.Create(options);

            #endregion
            _response.Result = shoppingCart;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}
