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
    }
}
