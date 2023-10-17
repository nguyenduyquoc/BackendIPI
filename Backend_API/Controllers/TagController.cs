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
    public class TagController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public TagController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // // GET LIST OF TAGs
        [HttpGet]
        [Route("get_tag")]
        public async Task<ActionResult<IEnumerable<TagDTO>>> Index()
        {
            var tags = await _context.Tags
                .ToListAsync();

            if (tags == null || tags.Count == 0)
            {
                return NotFound();
            }

            var tagDTOs = _mapper.Map<List<TagDTO>>(tags);

            return Ok(tagDTOs);
        }

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<TagDTO>> GetTag(int id)
        {
            if (_context.Tags == null)
            {
                return NotFound();
            }
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            // Map to DTO
            var tagDTO = _mapper.Map<TagDTO>(tag);
            return Ok(tagDTO);
        }

        // CREATE NEW TAG
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<TagDTO>> PostTag(TagCreateModel tagData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var tags = _context.Tags.ToList();
            //Check if tag with the same name already exists
            if (tags.Any(c => string.Equals(c.Name, tagData.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("A tag with the same name already exists.");
            }

            //Map 
            var tag = _mapper.Map<Tag>(tagData);

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            // Map the created Tag back to TagDTO and return it in the response
            var createdTagDTO = _mapper.Map<TagDTO>(tag);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, createdTagDTO);
        }


        // DELETE 
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            if (_context.Tags == null)
            {
                return NotFound();
            }
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutTag(int id, TagCreateModel tagData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Check if the tag with the given id exists in the database
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            // Check for duplicate tag name (ignore the current tag)
            if (_context.Tags.Any(c => c.Name == tagData.Name && c.Id != id))
            {
                return BadRequest("A tag with the same name already exists.");
            }

            //Map the properties from the TagDTO to the existing Tag entity
            _mapper.Map(tagData, tag);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        private bool TagExists(int id)
        {
            return (_context.Tags?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
