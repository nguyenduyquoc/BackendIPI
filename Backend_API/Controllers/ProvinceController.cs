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
    public class ProvinceController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public ProvinceController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // // GET LIST OF PROVINCE
        [HttpGet]
        [Route("get_provinces")]
        public async Task<ActionResult<IEnumerable<ProvinceDTO>>> Index()
        {
            var provinces = await _context.Provinces
                .Include(c => c.Country)
                .Include(c => c.Districts)
                .ToListAsync();

            if (provinces == null || provinces.Count == 0)
            {
                return NotFound();
            }

            var provinceDTOs = _mapper.Map<List<ProvinceDTO>>(provinces);

            return Ok(provinceDTOs);
        }

        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<ProvinceDTO>> GetProvince(int id)
        {
            if (_context.Provinces == null)
            {
                return NotFound();
            }
            var province = await _context.Provinces
                .Include(c => c.Country)
                .Include(c => c.Districts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (province == null)
            {
                return NotFound();
            }

            // Map to DTO
            var provinceDTO = _mapper.Map<ProvinceDTO>(province);
            return Ok(provinceDTO);
        }

        // CREATE NEW PROVINCE
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<ProvinceDTO>> PostProvince(ProvinceDTO provinceDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var provinces = _context.Provinces.ToList();
            //Check if province with the same name already exists
            if (provinces.Any(c => c.Name == provinceDTO.Name))
            {
                return BadRequest("A province with the same name already exists.");
            }

            //Map 
            var province = _mapper.Map<Province>(provinceDTO);

            _context.Provinces.Add(province);
            await _context.SaveChangesAsync();

            // Map the created Province back to ProvinceDTO and return it in the response
            var createdProvincesDTO = _mapper.Map<ProvinceDTO>(province);
            return CreatedAtAction(nameof(GetProvince), new { id = province.Id }, createdProvincesDTO);
        }


        // DELETE 
        /*
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteProvince(int id)
        {
            if (_context.Provinces == null)
            {
                return NotFound();
            }
            var province = await _context.Provinces.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }

            _context.Provinces.Remove(province);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        */

        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutProvince(int id, ProvinceDTO provinceDTO)
        {
            if (ModelState.IsValid)
            {
                return BadRequest();
            }

            //Check if the province with the given id exists in the database
            var province = await _context.Provinces.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }

            // Check for duplicate province name (ignore the current province)
            if (_context.Provinces.Any(c => c.Name == provinceDTO.Name && c.Id != id))
            {
                return BadRequest("A province with the same name already exists.");
            }

            //Map the properties from the ProvinceDTO to the existing Province entity
            _mapper.Map(provinceDTO, province);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProvinceExists(id))
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

        private bool ProvinceExists(int id)
        {
            return (_context.Provinces?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
