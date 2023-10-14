using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public PublisherController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // // GET LIST OF PUBLISHER
        [HttpGet]
        [Route("get_publisher")]
        public async Task<ActionResult<IEnumerable<PublisherDTO>>> Index()
        {
            var publishers = await _context.Publishers
                .ToListAsync();

            if (publishers == null || publishers.Count == 0)
            {
                return NotFound();
            }

            var publisherDTOs = _mapper.Map<List<PublisherDTO>>(publishers);

            return Ok(publisherDTOs);
        }

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<PublisherDTO>> GetPublisher(int id)
        {
            if (_context.Publishers == null)
            {
                return NotFound();
            }
            var publisher = await _context.Publishers
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {
                return NotFound();
            }

            // Map to DTO
            var publisherDTO = _mapper.Map<PublisherDTO>(publisher);
            return Ok(publisherDTO);
        }

        // CREATE NEW PUBLISHER
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<PublisherDTO>> PostPublisher(PublisherDTO publisherDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Check if publisher with the same name already exists
            if (_context.Publishers.Any(c => c.Name == publisherDTO.Name))
            {
                return BadRequest("A publisher with the same name already exists.");
            }

            //Map 
            var publisher = _mapper.Map<Entities.Publisher>(publisherDTO);

            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            // Map the created Publisher back to PublisherDTO and return it in the response
            var createdPublisherDTO = _mapper.Map<PublisherDTO>(publisher);
            return CreatedAtAction(nameof(GetPublisher), new { id = publisher.Id }, createdPublisherDTO);
        }


        // DELETE 
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            if (_context.Publishers == null)
            {
                return NotFound();
            }
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutPublisher(int id, PublisherDTO publisherDTO)
        {
            if (id != publisherDTO.Id)
            {
                return BadRequest("The id in the URL does not match the id in the request body.");
            }

            //Check if the district with the given id exists in the database
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }

            //Map the properties from the PublisherDTO to the existing Publisher entity
            _mapper.Map(publisherDTO, publisher);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublisherExists(id))
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

        private bool PublisherExists(int id)
        {
            return (_context.Publishers?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
