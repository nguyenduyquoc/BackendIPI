using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        [Route("get_categories_1")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> Index1(int? limit)
        {
            var query = _context.Categories
                .Include(c => c.Products)
                .Include(c => c.InverseParent)
                .Include(c => c.Parent)
                .Where(c => c.DeletedAt == null);
            // Check if the limit parameter is provided
            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            var categories = await query
                .Where(c => c.ParentId == null)
                .ToListAsync();

            if (categories == null || categories.Count == 0)
            {
                return NotFound();
            }

            var categoryDTOs = _mapper.Map<List<CategoryDTO>>(categories);

            return Ok(categoryDTOs);
        }

        [HttpGet]
        [Route("get_categories_2")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> Index2(int? limit)
        {
            var query = _context.Categories
                .Include(c => c.Products)
                .Include(c => c.InverseParent)
                .Where(c => c.DeletedAt == null);
            // Check if the limit parameter is provided
            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            var categories = await query
                .Where(c => c.ParentId != null)
                .ToListAsync();

            if (categories == null || categories.Count == 0)
            {
                return NotFound();
            }

            var categoryDTOs = _mapper.Map<List<CategoryDTO>>(categories);

            return Ok(categoryDTOs);
        }

        // GET LIST OF CATEGORY THAT HAVE BEEN DELETED
        [HttpGet]
        [Route("categories_havebeen_delete")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetDeletedCategories()
        {
            var deletedCategories = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.InverseParent)
                .Include(c => c.Parent)
                .Where(c => c.DeletedAt != null)
                .ToListAsync();

            if (deletedCategories == null || deletedCategories.Count == 0)
            {
                return NotFound();
            }

            // Map Category entities to CategoryDTO
            var deletedCategoryDTOs = _mapper.Map<List<CategoryDTO>>(deletedCategories);
            return deletedCategoryDTOs;
        }

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<CategoryDTO>> Get(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.InverseParent)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);


            if (category == null || category.DeletedAt != null)
            {
                return NotFound();
            }

            //Map Category to CategoryDTO
            var categoryDTO = _mapper.Map<CategoryDTO>(category);

            return Ok(categoryDTO);
        }



        // CREAT NEW A CATEGORY
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<CategoryDTO>> Create(CategoryDTO data)
        {
            if (ModelState.IsValid)
            {
                //Check if category with the same name already exists
                if (_context.Categories.Any(c => c.Name == data.Name))
                {
                    return BadRequest("A category with the same name already exists.");
                }
                //Map CategoryDTO to Category
                var category = _mapper.Map<Category>(data);

                // Set the CreatedAt property to the current date and time
                category.CreatedAt = DateTime.UtcNow;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // Map the created category back to CategoryDTO and return it in the response
                var createdCategoryDTO = _mapper.Map<CategoryDTO>(category);
                return CreatedAtAction(nameof(Get), new { id = category.Id }, createdCategoryDTO);
            }
            return BadRequest();
        }


        // DELETE CATEGORY TO TRASH ( dua vao thung rac)
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            //_context.Categories.Remove(category);
            category.DeletedAt = DateTime.UtcNow; //Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PERMANENTLY DELETED ( xoa han)
        [HttpDelete]
        [Route("permanently_deleted")]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (category.DeletedAt == null)
            {
                return BadRequest("Cannot delete");
            }
            else
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }


            return NoContent();
        }

        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutCategory(int id, CategoryDTO categoryDTO)
        {
            if(ModelState.IsValid)
            {
                if (id != categoryDTO.Id)
                {
                    return BadRequest("The id in the URL does not match the id in the request body.");
                }

                //Check if the category with the given id exists in the database
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                //Map the properties from the CategoryDTO to the existing Category entity
                _mapper.Map(categoryDTO, category);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            return BadRequest(ModelState);
        }


        // RESTORE
        [HttpPatch]
        [Route("restore")]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if the category is already restored (DeletedAt is null)
            if (category.DeletedAt == null)
            {
                return BadRequest("The category is already restored.");
            }

            // Restore the category by setting DeletedAt to null
            category.DeletedAt = null;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
