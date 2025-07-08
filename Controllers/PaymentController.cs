using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoApi.Data;
using RedMangoApi.Models;
using Stripe;

namespace RedMangoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public ApiResponse _response;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _response = new();
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId) //We need userID so we can return shopping cart of user and its CartTotal value
        {
            ShoppingCart cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems.Count() == 0 || cart.CartItems == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            #region Create Payment Intent
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            cart.CartTotal = cart.CartItems.Sum(c => c.Quantity * c.MenuItem.Price);
            PaymentIntentCreateOptions options = new()
            {
                Amount = (int)(cart.CartTotal * 100),
                Currency = "usd",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
            };
            PaymentIntentService service = new();
            PaymentIntent response = service.Create(options);

            cart.StripePaymentIntentId = response.Id;
            cart.ClientSecret = response.ClientSecret;
            #endregion

            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.Result = cart;
            return Ok(_response);
        }
    }
}
