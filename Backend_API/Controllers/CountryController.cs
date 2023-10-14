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
    public class CountryController : ControllerBase
    {
        private readonly BookstoreContext _context;

        private readonly IMapper _mapper;

        public CountryController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // // GET LIST OF COUNTRY
        [HttpGet]
        [Route("get_countries")]
        public async Task<ActionResult<IEnumerable<CountryDTO>>> GetCountries()
        {
            var countries = await _context.Countries
                .Include(c => c.Provinces)
                .ToListAsync();

            if (countries == null || countries.Count == 0)
            {
                return NotFound();
            }

            //Map to DTO
            var countryDTOs = _mapper.Map<List<CountryDTO>>(countries);
            return countryDTOs;
        }

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<CountryDTO>> GetCountry(int id)
        {
            if (_context.Countries == null)
            {
                return NotFound();
            }
            var country = await _context.Countries
                .Include(c => c.Provinces)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (country == null)
            {
                return NotFound();
            }

            // Map to DTO
            var countryDTO = _mapper.Map<CountryDTO>(country);
            return countryDTO;
        }

        // CREATE NEW COUNTRY
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<CountryDTO>> PostCountry(CountryDTO countryDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Check if country with the same name already exists
            if (_context.Countries.Any(c => c.Name == countryDTO.Name))
            {
                return BadRequest("A country with the same name already exists.");
            }

            //Map CountryDTO to Country
            var country = _mapper.Map<Country>(countryDTO);

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            // Map the created country back to CountryDTO and return it in the response
            var createdCountryDTO = _mapper.Map<CountryDTO>(country);
            return CreatedAtAction(nameof(GetCountries), new { id = country.Id }, createdCountryDTO);
        }


        // DELETE 
        /*
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            if (_context.Countries == null)
            {
                return NotFound();
            }
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        */

        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutCountry(int id, CountryDTO countryDTO)
        {
            if (id != countryDTO.Id)
            {
                return BadRequest("The id in the URL does not match the id in the request body.");
            }

            //Check if the country with the given id exists in the database
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            //Map the properties from the CountryDTO to the existing Country entity
            _mapper.Map(countryDTO, country);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
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

        private bool CountryExists(int id)
        {
            return (_context.Countries?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
