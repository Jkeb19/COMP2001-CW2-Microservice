using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2001_microservice.Data;
using _2001_microservice.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

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
                Username = profile.User.Username,
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
        public string UnitPreferences { get; set; }
        public string MarketingLanguage { get; set; }
        public string TimePreferences { get; set; }
    }

    private bool AuthenticateUser(string username, string password)
    {
        var user = _context.Users
            .Include(u => u.UserProfiles)
            .Include(u => u.UserPreferences)
            .FirstOrDefault(u => u.Username == username);

        return user != null && user.Password == password;
    }



    [HttpPut("update")]
    public IActionResult UpdateUser([FromBody] UpdateUserRequest request)
    {
        string username = request.Username;
        string password = request.Password;

        var isUserAuthenticated = AuthenticateUser(username, password);

        if (!isUserAuthenticated)
        {
            return Unauthorized("Invalid username or password.");
        }


        try
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(u => u.User.Username == username);
            var userPreferences = _context.UserPreferences.FirstOrDefault(u => u.User.Username == username);
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (request.AboutMe != "string")
            {
                userProfile.AboutMe = request.AboutMe;
            }

            if (request.EmailAddress != "string")
            {
                user.EmailAddress = request.EmailAddress;
            }

            if (request.MarketingLanguage != "string")
            {
                userPreferences.MarketingLanguage = request.MarketingLanguage;
            }

            if (request.TimePreferences != "string")
            {
                userPreferences.TimePreferences = request.TimePreferences;
            }
            
            if (request.UnitPreferences != "string")
            {
                userPreferences.UnitPreferences = request.UnitPreferences;
            }

            _context.SaveChanges();

            return Ok("User data updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to update user data. {ex.Message}");
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
    }

    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        if (_context.Users.Any(u => u.Username == request.Username || u.EmailAddress == request.EmailAddress))
        {
            return Conflict("Username or email address is already taken.");
        }

        try
        {
            var lastUser = _context.Users.OrderByDescending(u => u.UserId).FirstOrDefault();

            int nextUserId = (lastUser != null) ? lastUser.UserId + 1 : 1;

            var user = new Users
            {
                UserId = nextUserId,
                Username = request.Username,
                Password = request.Password, 
                EmailAddress = request.EmailAddress

            };

            
            var userProfile = new UserProfiles
            {
                UserId = nextUserId,
                ProfileId = nextUserId
                
            };

            var userPreferences = new UserPreferences
            {
                UserId = nextUserId,
                PrefID = nextUserId
            };

            _context.Users.Add(user);
            _context.UserProfiles.Add(userProfile);
            _context.UserPreferences.Add(userPreferences);

            _context.SaveChanges();

            return Ok($"User {request.Username} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create user. {ex.Message}");
        }
    }
}