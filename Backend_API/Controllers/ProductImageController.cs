using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public ProductImageController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // // GET LIST OF PRODUCTIMAGE
        [HttpGet]
        [Route("get_productImage")]
        public async Task<ActionResult<IEnumerable<ProductImageDTO>>> Index()
        {
            var productImages = await _context.ProductImages
                .Include(p => p.Product)
                .ToListAsync();

            if (productImages == null || productImages.Count == 0)
            {
                return NotFound();
            }

            var productImageDTOs = _mapper.Map<List<ProductImageDTO>>(productImages);

            return Ok(productImageDTOs);
        }

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<ProductImageDTO>> GetProductImage(int id)
        {
            if (_context.ProductImages == null)
            {
                return NotFound();
            }
            var productImage = await _context.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productImage == null)
            {
                return NotFound();
            }

            // Map to DTO
            var productImageDTO = _mapper.Map<ProductImageDTO>(productImage);
            return Ok(productImageDTO);
        }

        // CREATE NEW PRODUCTIMAGE
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<ProductImageDTO>> PostProductImage(ProductImageCreateModel productImageModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var productImages = _context.ProductImages.ToList();
            //Check if productImage with the same url already exists
            if (productImages.Any(p => string.Equals(p.Url, productImageModel.Url, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("A productImage with the same url already exists.");
            }

            //Map 
            var productImage = _mapper.Map<ProductImage>(productImageModel);

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            // Map the created ProductImage back to ProductImageDTO and return it in the response
            var createdProductImageDTO = _mapper.Map<ProductImageDTO>(productImage);
            return CreatedAtAction(nameof(GetProductImage), new { id = productImage.Id }, createdProductImageDTO);
        }


        // DELETE 
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            if (_context.ProductImages == null)
            {
                return NotFound();
            }
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }

            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutProductImage(int id, ProductImageDTO productImageDTO)
        {
            if (id != productImageDTO.Id)
            {
                return BadRequest("The id in the URL does not match the id in the request body.");
            }

            //Check if the productImage with the given id exists in the database
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }

            //Map the properties from the ProvinceDTO to the existing Province entity
            _mapper.Map(productImageDTO, productImage);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductImageExists(id))
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

        private bool ProductImageExists(int id)
        {
            return (_context.ProductImages?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
