using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NotificationAPI;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace LoginControllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserDBContext _context;

        public AuthController(IConfiguration configuration, UserDBContext context)
        {
            _configuration = configuration;
            _context = context;
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

        //[Authorize]
[HttpPost("send-email")]
public IActionResult SendEmail([FromBody] EmailRequest request)
{
    try
    {
        // Validate request
        if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
        {
            return BadRequest("Invalid email address");
        }
        
        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("Message cannot be empty");
        }
        
        // Retrieve SMTP settings from configuration
        var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        // Send email
        using (var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port))
        {
            smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);
            smtpClient.EnableSsl = true; // Enable SSL if required

            var message = new MailMessage
            {
                From = new MailAddress(smtpSettings.Username), // Use your SMTP username as the sender
                Subject = "Subject of the email",
                Body = request.Message
            };
            
            message.To.Add(request.Email);
            
            smtpClient.Send(message);
        }

        return Ok("Email sent successfully");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Failed to send email: {ex.Message}");
    }
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


        //method checks if username already exists...
        private async Task<bool> UserExists(string username)
        {
            return await _context.RegisterModels.AnyAsync(u => u.Username == username);
        }

        //Method to check if email already exists...
        private async Task<bool> EmailExists(string email)
        {
            return await _context.RegisterModels.AnyAsync(u => u.Email == email);
        }

        // Method to authenticate user....
        private async Task<RegisterModel> AuthenticateUser(string username, string password)
        {
            // Find the user...
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
