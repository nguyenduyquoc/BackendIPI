using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BookstoreContext _context;

        private readonly IConfiguration _configuration;

        private readonly IMapper _mapper;

        public AuthController(BookstoreContext context, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(UserRegisterModel registerModel)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if email is unique
                if (await _context.Users.AnyAsync(u => u.Email == registerModel.Email))
                {
                    return BadRequest(new { Message = "Email is already registered" });
                }

                // Hash and salt the password
                var salt = BCrypt.Net.BCrypt.GenerateSalt(6);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerModel.Password, salt);

                // Create new user
                var user = new User
                {
                    Fname = registerModel.Fname,
                    Lname = registerModel.Lname,
                    Email = registerModel.Email,
                    Password = hashedPassword, // Store hashed password
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateUserJWT(user);

                var userDTO = _mapper.Map<UserDTO>(user);

                return Ok(new { Token = token, User = userDTO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(UserLoginModel loginModel)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginModel.Email);
                if (user == null)
                {
                    return Unauthorized();
                }

                bool verifiedPassword = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password);
                if (!verifiedPassword)
                {
                    return Unauthorized();
                }

                var token = GenerateUserJWT(user);

                var userDTO = _mapper.Map<UserDTO>(user);

                return Ok(new { Token = token, User = userDTO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while logging in: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("admin_login")]
        public async Task<IActionResult> AdminLogin(UserLoginModel loginModel)
        {
            try
            {
                var admin = await _context.Admins
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(a => a.Email == loginModel.Email);

                if (admin == null)
                {
                    return Unauthorized();
                }

                bool verifiedPassword = BCrypt.Net.BCrypt.Verify(loginModel.Password, admin.Password);
                if (!verifiedPassword)
                {
                    return Unauthorized();
                }

                var token = GenerateAdminJWT(admin);

                var adminDTO = _mapper.Map<AdminDTO>(admin);

                return Ok(new { api_token = token, User = adminDTO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while logging in: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                var user = await _context.Users
                    .Include(u => u.UserAddresses)
                        .ThenInclude(ua => ua.District)
                            .ThenInclude(d => d.Province)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound();
                }

                var userDTO = _mapper.Map<UserDTO>(user);

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("admin_profile")]
        [Authorize(Roles = "MANAGER, STAFF")]
        public async Task<IActionResult> AdminProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                var admin = await _context.Admins
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(a => a.Id == userId);

                if (admin == null)
                {
                    return NotFound();
                }

                var adminDTO = _mapper.Map<AdminDTO>(admin);

                return Ok(adminDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        private string GenerateUserJWT(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var signatureKey = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var payloads = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role, "user")
            };
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                payloads,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:LifeTime"])),
                signingCredentials: signatureKey
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateAdminJWT(Admin admin)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var signatureKey = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var payloads = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,admin.Id.ToString()),
                new Claim(ClaimTypes.Email,admin.Email),
                new Claim(ClaimTypes.Role, admin.Role.Name)
            };
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                payloads,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:LifeTime"])),
                signingCredentials: signatureKey
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
