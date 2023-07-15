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
    public class ShoppingCartController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly ApplicationDbContext _db;

        public ShoppingCartController(ApplicationDbContext db)
        {
            _response = new();
            _db = db;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {
            ShoppingCart shoppingCart = _db.ShoppingCarts.Include(u => u.CartItems).FirstOrDefault(u => u.UserId == userId);
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == menuItemId);
            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            if (shoppingCart == null && updateQuantityBy > 0)
            {
                //Create shopping cart and add cart item
                ShoppingCart newCart = new() { UserId = userId };
                _db.ShoppingCarts.Add(newCart);
                _db.SaveChanges();
                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null
                };
                _db.CartItems.Add(newCartItem);
                _db.SaveChanges();

            }
            else
            {
                //Shopping cart exists
                CartItem CartItemInCart = shoppingCart.CartItems.FirstOrDefault(u => u.MenuItemId == menuItemId);
                if(CartItemInCart == null)
                {
                    //item does not exist  in current cart
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };
                    _db.CartItems.Add(newCartItem);
                    _db.SaveChanges();
                }
                else
                {
                    //Item already exist in cart => update quantity
                    int newQuantity = CartItemInCart.Quantity + updateQuantityBy;
                    if (updateQuantityBy == 0 || updateQuantityBy < 0)
                    {
                        //Remove cart item
                        _db.CartItems.Remove(CartItemInCart);
                        if(shoppingCart.CartItems.Count() == 1)
                        {
                            _db.ShoppingCarts.Remove(shoppingCart);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        CartItemInCart.Quantity = newQuantity;
                        _db.SaveChanges();
                    }
                }

            }
            return _response;
        }
    }
}
