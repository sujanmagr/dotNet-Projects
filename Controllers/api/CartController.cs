using FoodY.Data;
using FoodY.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoodY.Controllers.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const string CartIdCookieName = "CartId";
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<CartController> _logger; // Add logger


        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager,
           ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("AddItemInCart")]
        public async Task<IActionResult> AddItemInCart([FromBody] CartItemRequest request)
        {
            var cartId = GetOrCreateCartId();

            if (request == null || request.Quantity <= 0 || request.ProductId <= 0)
            {
                return BadRequest("Invalid request parameters.");
            }

            // Check if the user has a cart
            var cart = await _context.Carts
                                     .Include(c => c.CartDetails)
                                     .FirstOrDefaultAsync(c => c.Name == cartId && c.Status == "active");

            // If the cart doesn't exist, create a new one
            if (cart == null)
            {
                cart = new Cart
                {
                    Name = cartId,
                    UserId = request.UserId,
                    Status = "active",
                    CreatedDate = DateTimeOffset.Now,
                    CartDetails = new List<CartDetail>()
                };
                _context.Carts.Add(cart);
            }

            // Check if the product is already in the cart
            var cartDetail = cart.CartDetails.FirstOrDefault(cd => cd.ProductId == request.ProductId);
            if (cartDetail != null)
            {
                // Update quantity if the product is already in the cart
                cartDetail.Quantity += request.Quantity;
            }
            else
            {
                // Add a new CartDetail for the product
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                var newCartDetail = new CartDetail
                {
                    Cart = cart,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Price = product.Price,
                    CreatedDate = DateTimeOffset.Now,
                    UserId = request.UserId
                };

                cart.CartDetails.Add(newCartDetail); // Add to the cart's CartDetails collection
            }

            // Save all changes to the database
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log exception with detailed information
                _logger.LogError(ex, "Error saving changes to the database for UserId: {UserId}, ProductId: {ProductId}", request.UserId, request.ProductId);
                return StatusCode(500, "Internal server error while saving changes. Please try again later.");
            }

            return Ok(cart);
        }



        // Fetch Cart Details

        public async Task<IActionResult> Index()
        {
            // Get the currently signed-in user's ID
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Or redirect to login
            }
            var cart = await _context.Carts
                                     .Include(c => c.CartDetails)
                                     .ThenInclude(cd => cd.Product)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound();
            }
            return Ok(cart);
        }




        // Remove Cart Item
        [HttpDelete("remove/{cartDetailId}")]
        public async Task<IActionResult> RemoveCartItem(int cartDetailId)
        {
            var cartDetail = await _context.CartDetails.FindAsync(cartDetailId);
            if (cartDetail == null)
            {
                return NotFound();
            }

            _context.CartDetails.Remove(cartDetail);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private string GetOrCreateCartId()
        {
            if (!Request.Cookies.TryGetValue(CartIdCookieName, out string cartId))
            {
                cartId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append(CartIdCookieName, cartId, cookieOptions);
            }
            return cartId;
        }

        [HttpGet("GetCart1")]
        public async Task<IActionResult> GetCart1()
        {
            var cartId = GetOrCreateCartId();

            var cart = await _context.Carts
                .Where(c => c.Name == cartId)
                .Select(c => new
                {
                    cartDetails = c.CartDetails.Select(ci => new
                    {
                        id = ci.Id,
                        product = new
                        {
                            name = ci.Product.Name
                        },
                        price = ci.Product.Price,
                        quantity = ci.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return Ok(new { cartDetails = new List<object>() });
            }



            return Ok(cart);
        }



        // GET: api/Cart/{userId}
        [HttpGet("GetCart")]
        public async Task<IActionResult> GetCart(string userId)
        {
            if (userId == "guest")
            {
                return Ok(new { cartDetails = new List<object>() }); // Empty cart for guests
            }

            // Fetch the cart for the user by userId
            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    cartDetails = c.CartDetails.Select(ci => new
                    {
                        id = ci.Id,
                        product = new
                        {
                            name = ci.Product.Name
                        },
                        price = ci.Product.Price,
                        quantity = ci.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return NotFound(new { message = "Cart not found" });
            }

            return Ok(cart);
        }
        //// Update Quantity
        //[HttpPut("updateQuantity")]
        //public async Task<IActionResult> UpdateQuantity(int cartDetailId, string quantity)
        //{
        //    var cartDetail = await _context.CartDetails.FindAsync(cartDetailId);
        //    if (cartDetail == null)
        //    {
        //        return NotFound();
        //    }

        //    cartDetail.Quantity = quantity;
        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}
    }

}