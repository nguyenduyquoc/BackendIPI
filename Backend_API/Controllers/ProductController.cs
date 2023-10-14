using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public ProductController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET LIST OF PRODUCT THAT HAVE NOT BEEN DELETED
        [HttpPost]
        [Route("get_all_products")]
        public async Task<ActionResult<ListProductDTO>> GetProducts(
            [FromBody] ProductFiterRequest filterRequest
        )
        {
            var query = _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .Include(p => p.Tags)
                .Where(c => c.DeletedAt == null);

            // Apply filters
            if (filterRequest.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filterRequest.MinPrice.Value);
            }

            if (filterRequest.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filterRequest.MaxPrice.Value);
            }

            if (filterRequest.CategoryIds != null && filterRequest.CategoryIds.Any())
            {
                query = query.Where(p => p.Categories.Any(c => filterRequest.CategoryIds.Contains(c.Id)));
            }

            if (filterRequest.AuthorIds != null && filterRequest.AuthorIds.Any())
            {
                query = query.Where(p => filterRequest.AuthorIds.Contains(p.AuthorId));
            }

            if (filterRequest.PublisherIds != null && filterRequest.PublisherIds.Any())
            {
                query = query.Where(p => filterRequest.PublisherIds.Contains(p.PublisherId));
            }

            if (filterRequest.PublishYears != null && filterRequest.PublishYears.Any())
            {
                query = query.Where(p => filterRequest.PublishYears.Contains(p.PublishYear));
            }

            // Apply sorting
            switch (filterRequest.SortBy?.ToLower())
            {
                case "newest":
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
                case "oldest":
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case "highestprice":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "lowestprice":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "highestdiscount":
                    query = query.OrderByDescending(p => p.DiscountAmount ?? 0);
                    break;
                // Add other sorting options here
                default:
                    // Default sorting by created date (newest first)
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // Apply search query
            if (!string.IsNullOrEmpty(filterRequest.SearchQuery))
            {
                query = query.Where(p => p.Name.Contains(filterRequest.SearchQuery));
            }

            // Apply filtering by status if the 'status' parameter is provided
            if (filterRequest.Status.HasValue)
            {
                query = query.Where(c => c.Status == filterRequest.Status);
            }

            // Calculate the total number of items matching the criteria
            int totalItems = await query.CountAsync();

            // Initialize variables for totalPages and itemsPerPage
            int? totalPages = null;
            int itemsPerPage = 0;

            // Apply pagination if the 'page' and 'pageSize' parameters are provided
            if (filterRequest.Page.HasValue && filterRequest.PageSize.HasValue)
            {
                int currentPage = filterRequest.Page.Value;
                itemsPerPage = filterRequest.PageSize.Value;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                query = query.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);
            }

            var products = await query.ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            // Map entities to DTOs
            var productDTOs = _mapper.Map<List<ProductDTO>>(products);

            var response = new ListProductDTO
            {
                Products = productDTOs,
                TotalPages = totalPages,
                TotalItems = totalItems,
            };

            return response;
        }


        // GET LIST OF PRODUCT THAT HAVE NOT BEEN DELETED
        [HttpGet]
        [Route("deleted")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetDeletedProducts()
        {
            var deletedProducts = await _context.Products
                .Where(c => c.DeletedAt != null)
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .Include(p => p.Tags)
                .ToListAsync();

            if (deletedProducts == null || deletedProducts.Count == 0)
            {
                return NotFound();
            }

            // Map entities to DTOs
            var deletedProductDTOs = _mapper.Map<List<ProductDTO>>(deletedProducts);
            return deletedProductDTOs;
        }

        // GET BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            //Map to DTO
            var productsDTO = _mapper.Map<ProductDTO>(product);
            return productsDTO;
        }

        // FIND BY SLUG
        [HttpGet]
        [Route("get_by_slug")]
        public async Task<ActionResult<ProductDTO>> GetProductBySlug(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (product == null)
            {
                return NotFound();
            }

            // Map to DTO
            var productDTO = _mapper.Map<ProductDTO>(product);
            return productDTO;
        }

        // CREAT NEW PRODUCT
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<ProductDTO>> PostProduct(ProductDTO productDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Check if product with the same name already exists
            if (_context.Products.Any(c => c.Name == productDTO.Name))
            {
                return BadRequest("A product with the same name already exists.");
            }

            //Map ProductDTO to Product
            var product = _mapper.Map<Product>(productDTO);

            // Set the CreatedAt property to the current date and time
            product.CreatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Map the created product back to ProductDTO and return it in the response
            var createdProductDTO = _mapper.Map<ProductDTO>(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, createdProductDTO);
        }


        // DELETE CATEGORY TO TRASH ( dua vao thung rac)
        [HttpDelete]
        [Route("permanently_deleted")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Check if the product status is 0 (Pending) before allowing deletion
            if (product.Status != 0)
            {
                return BadRequest("Cannot delete a product with status other than 0 (Pending).");
            }

            product.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // RESTORE
        [HttpPatch("restore")]
        public async Task<IActionResult> RestoreDeletedProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Check if the product is already restored (DeletedAt is null)
            if (product.DeletedAt == null)
            {
                return BadRequest("The product is already restored.");
            }

            // Restore the product by setting DeletedAt to null
            product.DeletedAt = null;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        
        // UPDATE STATUS PRODUCT
        [HttpPatch("change_status")]
        public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] int status)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.Status = status;
            product.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the product's status.");
            }
        }

        // UPDATE PRODUCT
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutProduct(int id, ProductDTO productDTO)
        {
            if (id != productDTO.Id)
            {
                return BadRequest("The id in the URL does not match the id in the request body.");
            }

            //Check if the products with the given id exists in the database
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //Map the properties from the DTO to the existing entity
            _mapper.Map(productDTO, product);

            // Set the "updatedAt" property to the current datetime
            product.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

