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
    public class UserAddressController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;

        public UserAddressController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET LIST USERADDRESS
        [HttpGet]
        [Route("get_userAddresses")]
        public async Task<ActionResult<UserAddressList>> GetOrders(int? page, int? pageSize, bool? orderByDesc, string? searchName)
        {
            IQueryable<UserAddress> query = _context.UserAddresses;

           
            // Apply searching if the 'searchName' parameter is provided
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(u => u.User.Lname.Contains(searchName) || u.User.Fname.Contains(searchName));
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

            // Apply sorting by CreateAt if the 'orderByDescending' parameter is provided and true
            if (orderByDesc.HasValue && orderByDesc.Value)
            {
                query = query.OrderByDescending(u => u.UserId);
            }
            else
            {
                query = query.OrderBy(u => u.UserId);
            }

            var userAddresses = await query.ToListAsync();

            if (userAddresses == null || userAddresses.Count == 0)
            {
                return NotFound();
            }

            //Map entities to DTOs
            var userAddressDTOs = _mapper.Map<List<UserAddressDTO>>(userAddresses);

            var response = new UserAddressList
            {
                UserAddresses = userAddressDTOs,
                TotalPages = totalPages,
                TotalItems = totalItems,
            };

            return response;
        }


        // FIND BY ID
        [HttpGet]
        [Route("get_by_id")]
        public async Task<ActionResult<UserAddressDTO>> Get(int id)
        {
            var userAddress = await _context.UserAddresses
                .Include(u => u.User)
                .Include(u => u.District)
                .FirstOrDefaultAsync(u => u.Id == id);


            if (userAddress== null)
            {
                return NotFound();
            }

            //Map
            var userAddressDTO = _mapper.Map<UserAddressDTO>(userAddress);

            return Ok(userAddressDTO);
        }



        // CREAT NEW A USERADDRESS
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<UserAddressDTO>> Create(UserAddressCreateModel data)
        {
            if (ModelState.IsValid)
            {
                
                //Map 
                var userAddress = _mapper.Map<UserAddress>(data);

                _context.UserAddresses.Add(userAddress);
                await _context.SaveChangesAsync();

                // Map the created userAddress back to UserAddressDTO and return it in the response
                var createdUserAddressDTO = _mapper.Map<UserAddressDTO>(userAddress);
                return CreatedAtAction(nameof(Get), new { id = userAddress.Id }, createdUserAddressDTO);
            }
            return BadRequest();
        }


        // DELETE 
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteUserAddress(int id)
        {
            if (_context.UserAddresses == null)
            {
                return NotFound();
            }
            var userAddress = await _context.UserAddresses.FindAsync(id);
            if (userAddress == null)
            {
                return NotFound();
            }

            _context.UserAddresses.Remove(userAddress);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        

        // UPDATE 
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> PutUserAddress(int id, UserAddressEditModel dataModel)
        {
            if (ModelState.IsValid)
            {
                //Check if the userAddress with the given id exists in the database
                var userAddress = await _context.UserAddresses.FindAsync(id);
                if (userAddress == null)
                {
                    return NotFound();
                }

                //Map the properties from the userAddressDTO to the existing userAddress entity
                _mapper.Map(dataModel, userAddress);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserAddressExists(id))
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
            return BadRequest(ModelState);
        }


        
        private bool UserAddressExists(int id)
        {
            return (_context.UserAddresses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
