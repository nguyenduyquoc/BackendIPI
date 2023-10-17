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
    public class DistrictController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public DistrictController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // // GET LIST OF DISTRICT
        [HttpGet]
        [Route("get_districts")]
        public async Task<ActionResult<IEnumerable<DistrictDTO>>> Index()
        {
            var districts = await _context.Districts
                .Include(d => d.Province)
                .ToListAsync();

            if (districts == null || districts.Count == 0)
            {
                return NotFound();
            }

            var districtDTOs = _mapper.Map<List<DistrictDTO>>(districts);

            return Ok(districtDTOs);
        }

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<DistrictDTO>> GetDistrict(int id)
        {
            if (_context.Districts == null)
            {
                return NotFound();
            }
            var district = await _context.Districts
                .Include(d => d.Province)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (district == null)
            {
                return NotFound();
            }

            // Map to DTO
            var districtDTO = _mapper.Map<DistrictDTO>(district);
            return Ok(districtDTO);
        }

        // CREATE NEW DISTRICT
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<DistrictDTO>> PostDistrict(DistrictDTO districtDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var districts = _context.Districts.ToList();
            //Check if district with the same name already exists
            if (districts.Any(c => string.Equals(c.Name, districtDTO.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("A district with the same name already exists.");
            }

            //Map 
            var district = _mapper.Map<District>(districtDTO);

            _context.Districts.Add(district);
            await _context.SaveChangesAsync();

            // Map the created District back to DistrictDTO and return it in the response
            var createdDistrictDTO = _mapper.Map<DistrictDTO>(district);
            return CreatedAtAction(nameof(GetDistrict), new { id = district.Id }, createdDistrictDTO);
        }


        // DELETE 
        /*
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteDistrict(int id)
        {
            if (_context.Districts == null)
            {
                return NotFound();
            }
            var district = await _context.Districts.FindAsync(id);
            if (district == null)
            {
                return NotFound();
            }

            _context.Districts.Remove(district);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        */

        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutDistrict(int id, DistrictDTO districtDTO)
        {
            if (ModelState.IsValid)
            {
                return BadRequest();
            }

            //Check if the district with the given id exists in the database
            var district = await _context.Districts.FindAsync(id);
            if (district == null)
            {
                return NotFound();
            }

            // Check for duplicate district name (ignore the current district)
            if (_context.Districts.Any(d => d.Name == districtDTO.Name && d.Id != id))
            {
                return BadRequest("A category with the same name already exists.");
            }

            //Map the properties from the DistrictDTO to the existing District entity
            _mapper.Map(districtDTO, district);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DistrictExists(id))
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

        private bool DistrictExists(int id)
        {
            return (_context.Districts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
