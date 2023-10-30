using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly BookstoreContext _context;

        private readonly IMapper _mapper;

        public UserController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("liked_products")]
        [Authorize]
        public async Task<IActionResult> GetLikedProducts()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Query the database to fetch the user, including their liked products
                var user = await _context.Users
                    .Include(u => u.Products)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Extract the liked products from the user
                var likedProducts = user.Products;

                // You may want to map the likedProducts to DTOs if necessary
                var productDTOs = _mapper.Map<List<ProductDTO>>(likedProducts);

                return Ok(likedProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("like_product/{productId}")]
        [Authorize]
        public async Task<IActionResult> LikeProduct(int productId)
        {
            try
            {
                // Retrieve the user's ID from the claims in the token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Query the database to fetch the user, including their liked products
                var user = await _context.Users
                    .Include(u => u.Products) // Include the liked products
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Check if the product exists in the user's liked products
                var productToLike = await _context.Products.FindAsync(productId);

                if (productToLike == null)
                {
                    return NotFound("Product not found");
                }

                var likedProduct = user.Products.FirstOrDefault(p => p.Id == productId);

                if (likedProduct == null)
                {
                    // If the product is not liked, add it (like)
                    user.Products.Add(productToLike);
                }
                else
                {
                    // If the product is already liked, remove it (unlike)
                    user.Products.Remove(likedProduct);
                }

                await _context.SaveChangesAsync();

                return Ok("Product liked/unliked successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        // GET LIST USERs
        [HttpGet]
        [Route("get_users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> Index()
        {
            var query = await _context.Users
                    .Include(u => u.UserAddresses)
                        .ThenInclude(ua => ua.District)
                            .ThenInclude(d => d.Province)
                    .Where(u => u.DeletedAt == null)
                    .ToListAsync();

            

            if (query == null || query.Count == 0)
            {
                return NotFound();
            }

            var queryDTOs = _mapper.Map<List<UserDTO>>(query);

            return Ok(queryDTOs);
        }

        [HttpPut("update_profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProdile(UserProfileEdit model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Query the database to fetch the user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                
                _mapper.Map(model, user);

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok("User profile updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("order_history")]
        [Authorize]
        public async Task<IActionResult> GetOrderHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Query the database to fetch the user, including their liked products
                var user = await _context.Users
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.OrderProducts)
                            .ThenInclude(op => op.Product)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Extract the orders list from the user
                var orderHistory = user.Orders.OrderByDescending(o => o.CreatedAt);

                // You may want to map the likedProducts to DTOs if necessary
                var orderDTOs = _mapper.Map<List<OrderDTO>>(orderHistory);

                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("order_detail/{code}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetail(string code)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Query the database to find the order with the given code
                var order = await _context.Orders
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .Include(o => o.ReturnRequest)
                    .FirstOrDefaultAsync(o => o.Code == code);

                if (order == null)
                {
                    return NotFound("Order not found");
                }

                // Check if the order belongs to the current user
                if (order.UserId != userId)
                {
                    return NotFound("Order not found");
                }

                // You may want to map the order and its details to a DTO if necessary
                var orderDTO = _mapper.Map<OrderDTO>(order);

                return Ok(orderDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }

        [HttpGet("return_request_history")]
        [Authorize]
        public async Task<IActionResult> GetReturnRequestHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Query the database to fetch the user
                var user = await _context.Users
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.ReturnRequest)
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.OrderProducts)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Extract the return requests list from the user
                var returnRequestHistory = user.Orders
                    .Where(o => o.ReturnRequest != null)
                    .Select(o => o.ReturnRequest)
                    .OrderByDescending(r => r.CreatedAt);

                var returnRequestDTOs = _mapper.Map<List<ReturnRequestDTO>>(returnRequestHistory);

                return Ok(returnRequestDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("return_request/{id}")]
        [Authorize]
        public async Task<IActionResult> GetReturnRequestDetail(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Query the database to find the return with the given id
                var returnRequest = await _context.ReturnRequests
                    .Include(r => r.Order)
                        .ThenInclude(o => o.OrderProducts)
                            .ThenInclude(op => op.Product)
                    .Include(r => r.ReturnRequestImage)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (returnRequest == null)
                {
                    return NotFound("Return request not found");
                }

                // Check if the return request belongs to the current user
                if (returnRequest.Order.UserId != userId)
                {
                    return NotFound("Return request not found");
                }

                // You may want to map the DTO if necessary
                var returnRequestDTO = _mapper.Map<ReturnRequestDTO>(returnRequest);

                return Ok(returnRequestDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
