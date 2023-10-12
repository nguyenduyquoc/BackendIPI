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
    public class AuthorController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public AuthorController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET LIST AUTHORs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDTO>>> Index()
        {
            var authors = await _context.Authors
                .Include(a => a.Products)
                .ToListAsync();

            if(authors == null || authors.Count == 0)
            {
                return NotFound();
            }

            var authorDTOs = _mapper.Map<List<AuthorDTO>>(authors);

            return Ok(authorDTOs);
        }

        // fIND BY ID 
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<AuthorDTO>> Get(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Products)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            var authorDTO = _mapper.Map<AuthorDTO>(author);

            return Ok(authorDTO);
        }

        // CREAT NEW AUTHOR
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<AuthorDTO>> Create(AuthorDTO data)
        {
            if (ModelState.IsValid)
            {
                // Check if author with the same name already exists
                if (_context.Authors.Any(a => a.Name == data.Name))
                {
                    return BadRequest("A author with the same name already exists.");
                }
                
                var author = _mapper.Map<Author>(data);

                _context.Authors.Add(author);
                await _context.SaveChangesAsync();

                // Map the created author back to AuthorDTO and return it in the response
                var createdAuthorDTO = _mapper.Map<AuthorDTO>(author);
                return CreatedAtAction(nameof(Get), new { id = author.Id });
            }

            return BadRequest();
        }

        // DELETE AUTHOR
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            if (_context.Authors == null)
            {
                return NotFound();
            }
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutAuthor(int id, AuthorDTO authorDTO)
        {
            if (id != authorDTO.Id)
            {
                return BadRequest("The id in the URL does not match the id in the request body.");
            }

            //Check if the author with the given id exists in the database
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            //Map the properties from the AuthorDTO to the existing Author entity
            _mapper.Map(authorDTO, author);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(id))
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

        private bool AuthorExists(int id)
        {
            return (_context.Authors?.Any(a => a.Id == id)).GetValueOrDefault();
        }
    }
}
