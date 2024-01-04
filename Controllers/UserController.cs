// Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2001_microservice.Data;
using _2001_microservice.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public IActionResult GetUserProfile(int userId)
    {
        var userData = _context.UserProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => new
            {
                UserId = profile.UserId,
                Username = profile.User.Username, // Use UserName instead of Username
                ProfileId = profile.ProfileId,
                ProfilePicture = profile.ProfilePicture,
                AboutMe = profile.AboutMe,
                Archived = profile.Archived,
            })
            .FirstOrDefault();

        if (userData == null)
        {
            return NotFound($"User with UserId {userId} not found.");
        }

        return Ok(userData);
    }

    public class UpdateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? AboutMe { get; set; }
        public string EmailAddress { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public string MarketingLanguage { get; set; }
        public string TimePreferences { get; set; }

        // Add other properties as needed
    }

    private UserManager AuthenticateUser(string username, string password)
    {
        // Retrieve the user based on the provided credentials
        var user = _context.Users
            .Include(u => u.UserProfiles)
            .Include(u => u.UserPreferences)
            .FirstOrDefault(u => u.Username == username);

        // Check if the user exists and the password is correct
        if (user != null && VerifyPassword(user.hashedPassword, password))
        {
            return user;
        }

        return null;
    }

    [HttpPut("update")]
    public IActionResult UpdateUser([FromBody] UpdateUserRequest request)
    {
        // Step 1: Receive User Credentials
        string username = request.Username;
        string password = request.Password;

        // Step 2: Authenticate User
        var authenticatedUser = AuthenticateUser(username, password);

        if (authenticatedUser == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        // Step 3: Verify Password and Update User Data
        try
        {
            // Verify the entered password with the stored hashed password
            if (!VerifyPassword(authenticatedUser.PasswordHash, password))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Update user data based on the provided parameters
            authenticatedUser.UserProfiles.AboutMe = request.AboutMe;
            authenticatedUser.UserProfiles.EmailAddress = request.EmailAddress;
            authenticatedUser.UserProfiles.Image = request.ProfilePicture;
            authenticatedUser.UserPreferences.MarketingLanguage = request.MarketingLanguage;
            authenticatedUser.UserPreferences.TimePreferences = request.TimePreferences;

            // You can similarly update other properties like username and password if needed

            // Save changes to the database
            _context.SaveChanges();

            return Ok("User data updated successfully.");
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., validation errors, database errors)
            return BadRequest($"Failed to update user data. {ex.Message}");
        }
    }

    // Method to hash a password using SHA-512
    private string HashPassword(string password)
    {
        using (SHA512 sha512 = SHA512.Create())
        {
            byte[] hashedBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    // Method to verify a password against a hashed password
    private bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        return HashPassword(providedPassword) == hashedPassword;
    }
}
