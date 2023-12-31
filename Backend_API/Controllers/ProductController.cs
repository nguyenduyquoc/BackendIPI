﻿using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MANAGER, STAFF")]
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
        [AllowAnonymous]
        public async Task<ActionResult<ProductListDTO>> GetProducts(
            [FromBody] ProductFiterRequest filterRequest
        )
        {
            var query = _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.ProductImages)
                .Include(p => p.Tags)
                .Include(p => p.OrderProducts)
                    .ThenInclude(op => op.Order)
                        .ThenInclude(o => o.User)
                .Include(p => p.OrderProducts)
                    .ThenInclude(od => od.Review)
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
                case "highestrating":
                    query = query.OrderByDescending(p => p.OrderProducts
                    .Where(op => op.Review != null)
                    .Average(op => op.Review.Rating));
                    break;
                case "bestseller":
                    query = query.OrderByDescending(p => p.OrderProducts
                        .Where(op => op.Order != null && op.Order.Status == 4)
                        .Sum(op => op.Quantity));
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

            var response = new ProductListDTO
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

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductListDTO>> SearchProducts(int categoryId, string searchString, int? page, int? pageSize)
        {
            var query = _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.ProductImages)
                .Include(p => p.Tags)
                .Include(p => p.OrderProducts)
                    .ThenInclude(op => op.Order)
                        .ThenInclude(o => o.User)
                .Include(p => p.OrderProducts)
                    .ThenInclude(op => op.Review)
                .Where(c => c.DeletedAt == null);

            // Apply category filtering based on categoryId
            if (categoryId != 0)
            {
                // Retrieve category and its sub-categories
                var category = await _context.Categories
                    .Include(c => c.InverseParent)
                    .Where(c => c.Id == categoryId)
                    .FirstOrDefaultAsync();

                if (category != null)
                {
                    // Collect all category ids including sub-categories
                    var categoryIds = category.InverseParent
                        .Select(c => c.Id)
                        .ToList();
                    categoryIds.Add(category.Id);

                    query = query.Where(p => p.Categories.Any(c => categoryIds.Contains(c.Id)));
                }
            }
            // Apply search query
            if (!string.IsNullOrEmpty(searchString))
            {
                //searchString = searchString.ToLower(); // Convert to lowercase for case-insensitive search
                query = query.Where(p => p.Name.Contains(searchString));
            }

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

            var products = await query.ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            // Map entities to DTOs
            var productDTOs = _mapper.Map<List<ProductDTO>>(products);

            var response = new ProductListDTO
            {
                Products = productDTOs,
                TotalPages = totalPages,
                TotalItems = totalItems,
            };

            return response;
        }

        // GET BY ID
        [HttpGet]
        [Route("get_by_id")]
        [AllowAnonymous]
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
                .Include(p => p.OrderProducts)
                    .ThenInclude(op => op.Order)
                        .ThenInclude(o => o.User)
                .Include(p => p.OrderProducts)
                    .ThenInclude(op => op.Review)
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
        [Route("get_by_slug/{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDTO>> GetProductBySlug(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.ProductImages)
                .Include(p => p.OrderProducts)
                    .ThenInclude(op => op.Order)
                        .ThenInclude(o => o.User)
                .Include(p => p.OrderProducts)
                    .ThenInclude(op => op.Review)
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
        public async Task<ActionResult<ProductDTO>> PostProduct(ProductCreateModel productData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var products = _context.Products.ToList();
            //Check if product with the same name already exists
            if (products.Any(c => string.Equals(c.Name,  productData.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("A product with the same name already exists.");
            }

            //Map ProductDTO to Product
            var product = _mapper.Map<Product>(productData);

            //Manually set the associated categories based on their existing IDs
            var categoryIds = productData.CategoryIds;
            if(categoryIds.Any())
            {
                foreach (var categoryId in categoryIds)
                {
                    var category = await _context.Categories.FindAsync(categoryId);
                    if (category != null)
                    {
                        // Add the product to the category
                        category.Products.Add(product);
                    }
                }
            }

            //Manually set the associated tags based on their existing IDs
            var tagIds = productData.TagIds;
            if (tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        // Add the product to the category
                        tag.Products.Add(product);
                    }
                }
            }
           

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
        public async Task<IActionResult> PutProduct(int id, ProductEditModel productData)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            //Check if the products with the given id exists in the database
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            //Map the properties from the DTO to the existing entity
            _mapper.Map(productData, product);

            // Check for duplicate product name (ignore the current product)
            if (_context.Products.Any(p => p.Name == productData.Name && p.Id != id))
            {
                return BadRequest("A category with the same name already exists.");
            }

            // Create new product images
            foreach (var imageModel in productData.NewProductImages)
            {
                var newImage = _mapper.Map<ProductImage>(imageModel);
                product.ProductImages.Add(newImage);
            }

            // Remove product images
            foreach (var imageId in productData.RemoveProductImageIds)
            {
                var imageToRemove = _context.ProductImages.FirstOrDefault(img => img.Id == imageId);
                if (imageToRemove != null)
                {
                    _context.ProductImages.Remove(imageToRemove);
                    // You might also want to delete the image from storage here.
                }
            }

            // Handle categories
            foreach (var categoryId in productData.CategoryIds)
            {
                // Check if the product is already associated with the category
                var isAssociated = product.Categories.Any(c => c.Id == categoryId);

                if (!isAssociated)
                {
                    // The product is not associated with this category, add it
                    var category = await _context.Categories.FindAsync(categoryId);
                    if (category != null)
                    {
                        product.Categories.Add(category);
                    }
                }
            }


            // Remove the product from categories that are no longer associated
            foreach (var category in product.Categories.ToList())
            {
                if (!productData.CategoryIds.Contains(category.Id))
                {
                    product.Categories.Remove(category);
                }
            }

            // Handle tags
            foreach (var tagId in productData.TagIds)
            {
                // Check if the product is already associated with the tag
                var isAssociated = product.Tags.Any(t => t.Id == tagId);

                if (!isAssociated)
                {
                    // The product is not associated with this tag, add it
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        product.Tags.Add(tag);
                    }
                }
            }

            // Remove the product from tags that are no longer associated
            foreach (var tag in product.Tags.ToList())
            {
                if (!productData.TagIds.Contains(tag.Id))
                {
                    product.Tags.Remove(tag);
                }
            }

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

        [HttpGet("related_products/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductDTO>>> GetRelatedProducts(int productId, int limit)
        {
            var currentProduct = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (currentProduct == null)
            {
                return NotFound();
            }

            var relatedProducts = new List<ProductDTO>();
           

            // Step 2: Find products with similar categories
            relatedProducts.AddRange(await FindProductsByCategories(currentProduct.Categories));

            // Step 3: Find products with similar tags
            relatedProducts.AddRange(await FindProductsByTags(currentProduct.Tags));

            // Step 4: Rank related products based on relevance (you need to implement the ranking logic)

            // Step 5: Return the top related products
            var topRelatedProducts = relatedProducts
                .Where(p => p.Id != productId) // Exclude the original product
                .Take(limit)
                .ToList();

            return topRelatedProducts;
        }

        private async Task<List<ProductDTO>> FindProductsByAuthor(int authorId)
        {
            return await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.Tags)
                .Where(p => p.AuthorId == authorId)
                .Select(p => _mapper.Map<ProductDTO>(p))
                .ToListAsync();
        }

        private async Task<List<ProductDTO>> FindProductsByCategories(ICollection<Category> categories)
        {
            // Get the list of category IDs
            var categoryIds = categories.Select(c => c.Id).ToList();

            // Fetch the products in-memory and then filter in-memory
            var products = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.Tags)
                .Where(p => p.Categories.Any(c => categoryIds.Contains(c.Id)))
                .ToListAsync();

            // Now that we have the products in-memory, we can select and map them
            var productDTOs = products.Select(p => _mapper.Map<ProductDTO>(p)).ToList();

            return productDTOs;
        }

        private async Task<List<ProductDTO>> FindProductsByTags(ICollection<Tag> tags)
        {
            // Get the list of tag IDs
            var tagIds = tags.Select(t => t.Id).ToList();

            // Fetch the products in-memory and then filter in-memory
            var products = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Publisher)
                .Include(p => p.Categories)
                .Include(p => p.Tags)
                .Where(p => p.Tags.Any(t => tagIds.Contains(t.Id)))
                .ToListAsync();

            // Now that we have the products in-memory, we can select and map them
            var productDTOs = products.Select(p => _mapper.Map<ProductDTO>(p)).ToList();

            return productDTOs;
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // LAY TAT CA CAC NAM SAN XUAT BAN DE LOC
        [HttpGet("publish_years")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPublishYears()
        {
            var publishYears = await _context.Products
                .Where(p => p.PublishYear != 0)
                .Select(p => p.PublishYear)
                .Distinct()
                .OrderBy(year => year)
                .ToArrayAsync();

            return Ok(publishYears);
        }


        private decimal CalculateAverageRating(List<Review> reviews)
        {
            if (reviews == null || reviews.Count == 0)
            {
                return 0; // or another default value
            }

            decimal totalRating = reviews.Sum(r => r.Rating);
            return totalRating / reviews.Count;
        }
    }

}

