using System.ComponentModel.DataAnnotations;

namespace _2001_microservice.Models
{
    public class UserProfiles
    {
        [Key]
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string? ProfilePicture  { get; set; }
        public string? AboutMe { get; set; }
        public int Archived { get; set; }
        public Users User { get; set; }

    }
}
