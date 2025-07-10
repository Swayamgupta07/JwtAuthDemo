using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AutoMapper;
using JwtAuthDemo.Data;
using JwtAuthDemo.Model.DTO;
using JwtAuthDemo.Model;
using JwtAuthDemo.Services;
using JwtAuthDemo.Model.Entity;

namespace JwtAuthDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserRepository userRepository, ITokenService tokenService, IMapper mapper, ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync(); 
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching users");
                throw;
            }
        }

        [ResponseCache(Duration = 5)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserName) ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("Registration failed: Incomplete user data");
                    return BadRequest(new { message = "All fields are required" });
                }

                var userByUsername = await _userRepository.GetUserByUsernameAsync(dto.UserName);
                var userByEmail = await _userRepository.GetUserByEmailAsync(dto.Email);

                if (userByUsername != null)
                {
                    _logger.LogWarning("Registration failed: Username '{UserName}' already exists", dto.UserName);
                    return BadRequest(new { message = "Username already exists" });
                }

                if (userByEmail != null)
                {
                    _logger.LogWarning("Registration failed: Email '{Email}' already exists", dto.Email);
                    return BadRequest(new { message = "Email already exists" });
                }


                var user = _mapper.Map<AppUser>(dto);

                // Hash password
                var hasher = new PasswordHasher<AppUser>();
                user.Passwordhash = hasher.HashPassword(user, dto.Password);

                _logger.LogInformation("Registering user: {UserName}, Email: {Email}", user.UserName, user.Email);

                // Save user
                await _userRepository.RegisterUserAsync(user);

                _logger.LogInformation("User registered successfully: {UserName}", user.UserName);

                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during registration");
                throw; // Let middleware handle it
            }
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserloginDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("Login failed: Incomplete user data");
                    return BadRequest(new { message = "Username and password are required" });
                }

                var user = await _userRepository.GetUserByUsernameAsync(dto.UserName);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User '{UserName}' not found", dto.UserName);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                if (string.IsNullOrEmpty(user.Passwordhash))
                {
                    _logger.LogWarning("Login failed: User '{UserName}' has no password set", dto.UserName);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var hasher = new PasswordHasher<AppUser>();
                var passwordVerificationResult = hasher.VerifyHashedPassword(user, user.Passwordhash, dto.Password);
                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Login failed: Invalid password for user '{UserName}'", dto.UserName);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                _logger.LogInformation("User '{UserName}' logged in successfully", user.UserName);
                var token = _tokenService.CreateToken(user);
                user.Email = user.Email?.ToLowerInvariant(); // Ensure email is in lowercase for consistency

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Token = token;
                return Ok(new { user = userDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login");
                throw;
            }
        }

        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteUser([FromBody] UserDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.UserName))
                {
                    _logger.LogWarning("Delete failed: Incomplete user data");
                    return BadRequest(new { message = "Username is required" });
                }

                var user = await _userRepository.GetUserByUsernameAsync(dto.UserName);
                if (user == null)
                {
                    _logger.LogWarning("Delete failed: User '{UserName}' not found", dto.UserName);
                    return NotFound(new { message = "User not found" });
                }

                await _userRepository.DeleteUserAsync(user.Id);
                _logger.LogInformation("User '{UserName}' deleted successfully", dto.UserName);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during user deletion");
                throw;
            }
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UserloginDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("Password update failed: Incomplete user data");
                    return BadRequest(new { message = "Username and password are required" });
                }
                var user = await _userRepository.GetUserByUsernameAsync(dto.UserName);
                if (user == null)
                {
                    _logger.LogWarning("Password update failed: User '{UserName}' not found", dto.UserName);
                    return NotFound(new { message = "User not found" });
                }
                var hasher = new PasswordHasher<AppUser>();
                user.Passwordhash = hasher.HashPassword(user, dto.Password);
                await _userRepository.UpdatePasswordAsync(dto.UserName, user.Passwordhash);
                _logger.LogInformation("Password updated successfully for user: {UserName}", user.UserName);
                return Ok(new { message = "Password updated successfully" });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during password update");
                throw;
            }
        }
    }
 }
