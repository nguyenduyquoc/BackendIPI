using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public ReviewController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET LIST OF REVIEW
        [HttpGet]
        [Route("get_reviews")]
        public async Task<ActionResult<ReviewList>> GetReviews(int? page, int? pageSize, bool? orderByDesc, bool? editable)
        {
            IQueryable<Review> query = _context.Reviews
                .Include(r => r.OrderProduct)
                    .ThenInclude(r => r.Order)
                        .ThenInclude(r => r.User);

            // Calculate the total number of items matching the criteria
            int totalItems = await query.CountAsync();

            // Initialize variables for totalPages and itemsPerPage
            int? totalPages = null;
            int itemsPerPage = 0;

            // Apply pagination if the 'page' and 'pageSize' parameters are provided
            if (page.HasValue && pageSize.HasValue)
            {
                int currentPage = page.Value;
                itemsPerPage = pageSize.Value;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                query = query.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);
            }

            // Apply sorting by CreateAt if the 'orderByDescending' parameter is provided and true
            if (orderByDesc.HasValue && orderByDesc.Value)
            {
                query = query.OrderByDescending(o => o.CreatedAt);
            }
            else
            {
                query = query.OrderBy(o => o.CreatedAt);
            }

            var reviews = await query.ToListAsync();

            if (reviews == null || reviews.Count == 0)
            {
                return NotFound();
            }

            //Map entities to DTOs
            var reviewDTOs = _mapper.Map<List<ReviewDTO>>(reviews);

            var response = new ReviewList
            {
                Reviews = reviewDTOs,
                TotalPages = totalPages,
                TotalItems = totalItems,
            };

            return response;
        }


        

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<ReviewDTO>> Get(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.OrderProduct)
                    .ThenInclude(r => r.Order)
                        .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);


            if (review == null)
            {
                return NotFound();
            }

            //Map
            var reviewDTO = _mapper.Map<ReviewDTO>(review);

            return Ok(reviewDTO);
        }



        // CREAT NEW A REVIEW
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<ReviewDTO>> PostReview(ReviewCreateModel reviewData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Map ReviewCreateModel to Review
            var review = _mapper.Map<Review>(reviewData);

            // Set the CreatedAt property to the current date and time
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Map the created review back to ReviewDTO and return it in the response
            var createdReviewDTO = _mapper.Map<ReviewDTO>(review);
            return CreatedAtAction(nameof(Get), new { id = review.Id }, createdReviewDTO);
        }

        // DELETE 
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Reviews == null)
            {
                return NotFound();
            }
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
