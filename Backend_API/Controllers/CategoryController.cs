using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public CategoryController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET LIST OF CATEGORY THAT HAVE NOT BEEN DELETED
        [HttpGet]
        [Route("get-all_categories")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.InverseParent)
                .Include(c => c.Parent)
                .Where(c => c.DeletedAt == null)
                .ToListAsync();


            if (categories == null || categories.Count == 0)
            {
                return NotFound();
            }

            var categoryDTOs = _mapper.Map<List<CategoryDTO>>(categories);

            return Ok(categoryDTOs);
        }
    }
}
