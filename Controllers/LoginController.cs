// using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NotificationAPI;
using Microsoft.EntityFrameworkCore;
using PostmarkDotNet;
using Microsoft.AspNetCore.Mvc;

namespace LoginControllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserDBContext _context;
        private readonly PostmarkClient _postmarkClient;

        public AuthController(IConfiguration configuration, UserDBContext context)
        {
            _configuration = configuration;
            _context = context;
            _postmarkClient = new PostmarkClient("28f12f7b-d840-41b9-8b20-4ab43212155e");
        }
    [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Check if model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Does user exist?
            if (await UserExists(model.Username) || await EmailExists(model.Email))
            {
                return Conflict(new { Message = "Username or email already exists" });
            }

            // Create a new user....
            var newUser = new RegisterModel
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password // Save the password to db...PS,not harshed
            };

            // Save the new user to db...
            _context.RegisterModels.Add(newUser);
            await _context.SaveChangesAsync();

            // Return success response
            return Ok(new { Message = "Registration successful" });
        }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogin model)
    {
        try
        {
            // Authenticate the user with provided credentials
            var user = await AuthenticateUser(model.Username, model.Password);
            if (user == null)
            {
                // Return 401 Unauthorized if authentication fails...
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            // Generate token...
            var token = GenerateJwtToken(user.Username);
            return Ok(new
            {
                Message = $"Welcome, {user.Username}!",
                Token = token
            });
        }
        catch
        {
            // 500 Internal Server Error when an unexpected error occurs
            return StatusCode(500, new { Message = "An unexpected error occurred during login" });
        }
    }

    [HttpPost("send-email")]
    //[Authorize] 
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
                {
                    return BadRequest("Invalid email address");
                }

                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest("Message cannot be empty");
                }

                var emailService = new PostmarkEmailService("2a5833b7-c083-49fd-ade6-d348b4f42f99");
                var isEmailSent = await emailService.SendEmailAsync("dubem.egbo@saed.dev", request.Email, "Dubem's Test Email", request.Message);
                if (isEmailSent)
                {
                    return Ok("Email sent successfully");
                }
                else
                {
                    return StatusCode(500, "Failed to send email");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }

        public override UnauthorizedObjectResult Unauthorized(object value)
        {
            return base.Unauthorized(new { Message = "Please login to access this resource." });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        //method to check if username already exists...
        private async Task<bool> UserExists(string username)
        {
            return await _context.RegisterModels.AnyAsync(u => u.Username == username);
        }

        //to check if email already exists...
        private async Task<bool> EmailExists(string email)
        {
            return await _context.RegisterModels.AnyAsync(u => u.Email == email);
        }

        //authenticates the user....
        private async Task<RegisterModel> AuthenticateUser(string username, string password)
        {
            // Finds the user...
            var user = await _context.RegisterModels.FirstOrDefaultAsync(u => u.Username == username);
            
            // Check if user exists and the password matches
            if (user != null && user.Password == password)
            {
                return user; // Return the user if authentication is successful
            }
            
            return null; // Return null if user does not exist or password does not match
        }

        // Method generates JWT token
        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}