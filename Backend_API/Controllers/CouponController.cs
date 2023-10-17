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
    public class CouponController : ControllerBase
    {
        private readonly BookstoreContext _context;

        private readonly IMapper _mapper;

        public CouponController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // // GET LIST OF COUPONs HAVE NOT BEEN DELETED
        [HttpGet]
        [Route("get_coupons")]
        public async Task<ActionResult<IEnumerable<CouponDTO>>> GetCoupons()
        {
            var coupons = await _context.Coupons
                .Where(c => c.DeletedAt == null)
                .ToListAsync();

            if (coupons == null || coupons.Count == 0)
            {
                return NotFound();
            }

            var couponDTOs = _mapper.Map<List<CouponDTO>>(coupons);

            return Ok(couponDTOs);
        }


        // // GET LIST OF COUPONs HAVE BEEN DELETED
        [HttpGet]
        [Route("get_coupons_deleted")]
        public async Task<ActionResult<IEnumerable<CouponDTO>>> GetDeletedCoupons()
        {
            var coupons = await _context.Coupons
                .Where(c => c.DeletedAt != null)
                .ToListAsync();

            if (coupons == null || coupons.Count == 0)
            {
                return NotFound();
            }

            var couponDTOs = _mapper.Map<List<CouponDTO>>(coupons);

            return Ok(couponDTOs);
        }
        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<CouponDTO>> GetCoupon(int id)
        {
            if (_context.Coupons == null)
            {
                return NotFound();
            }
            var coupon = await _context.Coupons.FindAsync(id);

            if (coupon == null)
            {
                return NotFound();
            }

            var couponDTO = _mapper.Map<CouponDTO>(coupon);

            return couponDTO;
        }


        // FIND BY CODE
        [HttpGet]
        [Route("get_by_code")]
        public async Task<ActionResult<CouponDTO>> GetCouponByCode(string code)
        {
            var coupon = await _context.Coupons.SingleOrDefaultAsync(c => c.Code == code);

            if (coupon == null)
            {
                return NotFound("Coupon not found.");
            }

            var couponDTO = _mapper.Map<CouponDTO>(coupon);
            return couponDTO;
        }


        // CREAT NEW A COUPON
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<CouponDTO>> Create(CouponCreateModel data)
        {
            if (ModelState.IsValid)
            {
                var coupons = _context.Coupons.ToList();
                //Check if coupon with the same code already exists
                if (coupons.Any(c => string.Equals(c.Code, data.Code, StringComparison.OrdinalIgnoreCase )))
                {
                    return BadRequest("A coupon with the same code already exists.");
                }
                //Map CouponDTO to Coupon
                var coupon = _mapper.Map<Coupon>(data);

                // Set the CreatedAt property to the current date and time
                coupon.CreatedAt = DateTime.UtcNow;

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();

                // Map the created coupon back to CouponDTO and return it in the response
                var createdCouponDTO = _mapper.Map<CouponDTO>(coupon);
                return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, createdCouponDTO);
            }
            return BadRequest();
        }

        // DELETE COUPON TO TRASH ( dua vao thung rac)
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            if (_context.Coupons == null)
            {
                return NotFound();
            }
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            coupon.DeletedAt = DateTime.UtcNow; //Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PERMANENTLY DELETED ( xoa han)
        [HttpDelete]
        [Route("permanently_deleted")]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Coupons == null)
            {
                return NotFound();
            }
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            if (coupon.DeletedAt == null)
            {
                return BadRequest("Cannot delete");
            }
            else
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }


        // RESTORE
        [HttpPatch]
        [Route("restore")]
        public async Task<IActionResult> RestoreCoupon(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            // Check if the coupon is already restored (DeletedAt is null)
            if (coupon.DeletedAt == null)
            {
                return BadRequest("The coupon is already restored.");
            }

            // Restore the coupon by setting DeletedAt to null
            coupon.DeletedAt = null;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutCoupon(int id, CouponEditModel editData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            //Check if the coupon with the given id exists in the database
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }
            // Check for duplicate coupon name (ignore the current coupon)
            if (_context.Coupons.Any(c => c.Code == editData.Code && c.Id != id))
            {
                return BadRequest("A coupon with the same name already exists.");
            }

            //Map the properties from the CouponDTO to the existing Coupon entity
            _mapper.Map(editData, coupon);
            coupon.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CouponExists(id))
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

        private bool CouponExists(int id)
        {
            return (_context.Coupons?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
