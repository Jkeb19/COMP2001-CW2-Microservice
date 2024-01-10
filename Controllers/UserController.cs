using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2001_microservice.Data;
using _2001_microservice.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;
using System.Text;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    public UserController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
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
        public string ProfilePicture { get; set; }
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

            if (request.ProfilePicture != "string")
            {
                userProfile.ProfilePicture = request.ProfilePicture;
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
    public class ChangePasswordRequest
    {
        public string Username { get; set; }

        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    [HttpPut("change-password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        string username = request.Username;
        string oldPassword = request.OldPassword;
        string newPassword = request.NewPassword;

        var user = _context.Users.FirstOrDefault(u => u.Username == username);

        var authenticatedUser = AuthenticateUser(username, oldPassword);

        if (!authenticatedUser)
        {
            return Unauthorized("Invalid username or password.");
        }

        try
        {
            user.Password = newPassword;

            _context.SaveChanges();

            return Ok("Password changed successfully.");
        }
        catch (Exception ex)
        {

            return BadRequest($"Failed to change password. {ex.Message}");
        }
    }
    public class ArchiveAccountRequest
    {
        public int AdminId { get; set; }
        public string UsernameToArchive { get; set; }
    }

    [HttpPut("archive-account")]
    public IActionResult ArchiveAccount([FromBody] ArchiveAccountRequest request)
    {
        int adminId = request.AdminId;
        string usernameToArchive = request.UsernameToArchive;

        var isAdmin = _context.Admin.Any(a => a.AdminId == adminId);

        if (!isAdmin)
        {
            return Unauthorized("You do not have permission to archive accounts.");
        }

        try
        {
            var userProfile = _context.UserProfiles
                .Include(up => up.User)
                .Include(up => up.User.UserPreferences)
                .FirstOrDefault(up => up.User.Username == usernameToArchive);

            if (userProfile == null)
            {
                return NotFound($"User with username {usernameToArchive} not found.");
            }

            userProfile.Archived = 1;

            _context.Users.Remove(userProfile.User);
            _context.UserPreferences.Remove(userProfile.User.UserPreferences);

            _context.SaveChanges();

            return Ok($"Account for user {usernameToArchive} archived successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to archive account. {ex.Message}");
        }
    }
  
    public class SelfArchiveAccountRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    [HttpPut("self-archive-account")]
    public IActionResult SelfArchiveAccount([FromBody] SelfArchiveAccountRequest request)
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
            var userProfile = _context.UserProfiles
                .Include(up => up.User)
                .Include(up => up.User.UserPreferences)
                .FirstOrDefault(up => up.User.Username == username);

            if (userProfile == null)
            {
                return NotFound($"User with username {username} not found.");
            }

  
            userProfile.Archived = 1;

            _context.Users.Remove(userProfile.User);
            _context.UserPreferences.Remove(userProfile.User.UserPreferences);

            _context.SaveChanges();

            return Ok($"Your account has been archived successfully.");
        }
        catch (Exception ex)
        {

            return BadRequest($"Failed to archive your account. {ex.Message}");
        }
    }
    public class TrailRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TrailName { get; set; }
    }

    [HttpPost("addTrail")]
    public IActionResult AddTrail([FromBody] TrailRequest request)
    {
        
        var isUserAuthenticated = AuthenticateUser(request.Username, request.Password);

        if (!isUserAuthenticated)
        {
            return Unauthorized("Invalid username or password.");
        }

        try
        {
            
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                return NotFound($"User with username {request.Username} not found.");
            }
            
            var maxTrailId = _context.UserTrails.Max(t => t.TrailId);

            var newTrailId = (maxTrailId >= 0 && maxTrailId < 99999) ? maxTrailId + 1 : 0;

            var trail = new UserTrails
            {
                UserId = user.UserId,
                TrailName = request.TrailName,
                TrailId = newTrailId,

            };

            _context.UserTrails.Add(trail);
            _context.SaveChanges();

            return Ok("Trail added successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to add trail. {ex.Message}");
        }
    }
    [HttpGet("getTrails")]
    public IActionResult GetUserTrails(string username, string password)
    {
        var isUserAuthenticated = AuthenticateUser(username, password);

        if (!isUserAuthenticated)
        {
            return Unauthorized("Invalid username or password.");
        }

        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return NotFound($"User with username {username} not found.");
            }

            var userTrails = _context.UserTrails
                .Where(trail => trail.UserId == user.UserId)
                .Select(trail => new
                {
                    TrailId = trail.TrailId,
                    TrailName = trail.TrailName
                })
                .ToList();

            return Ok(userTrails);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to retrieve user trails. {ex.Message}");
        }
    }
    public class DeleteTrailRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int TrailId { get; set; }
    }

    [HttpDelete("deleteTrail")]
    public IActionResult DeleteTrail([FromBody] DeleteTrailRequest request)
    {
 
        var isUserAuthenticated = AuthenticateUser(request.Username, request.Password);

        if (!isUserAuthenticated)
        {
            return Unauthorized("Invalid username or password.");
        }

        try
        {

            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                return NotFound($"User with username {request.Username} not found.");
            }

            var trailToDelete = _context.UserTrails.FirstOrDefault(t => t.TrailId == request.TrailId && t.UserId == user.UserId);

            if (trailToDelete == null)
            {
                return NotFound($"Trail with TrailID {request.TrailId} not found or does not belong to the user.");
            }

            _context.UserTrails.Remove(trailToDelete);
            _context.SaveChanges();

            return Ok($"Trail with TrailID {request.TrailId} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to delete trail. {ex.Message}");
        }
    }
    [HttpGet("APIauthentication")]
    public async Task<IActionResult> FetchTokenFromApi([FromQuery] string email, [FromQuery] string password)
    {
        try
        {
 
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Both email and password are required.");
            }

            var apiUrl = "https://web.socem.plymouth.ac.uk/COMP2001/auth/api/users";

            
            var credentials = new
            {
                Email = email,
                Password = password
            };

            var credentialsJson = JsonConvert.SerializeObject(credentials);

            var content = new StringContent(credentialsJson, Encoding.UTF8, "application/json");

            var authResponse = await _httpClient.PostAsync(apiUrl, content);

           
            if (authResponse.IsSuccessStatusCode)
            {
                var token = await authResponse.Content.ReadAsStringAsync();

                return Ok(new { Token = token });
            }
            else
            {
                return Unauthorized("Authentication failed");
            }
        }
        catch (HttpRequestException ex)
        {
            return BadRequest($"Failed to fetch token from the API. {ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred. {ex.Message}");
        }
    }
}




