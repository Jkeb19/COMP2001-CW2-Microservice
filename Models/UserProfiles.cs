using System.ComponentModel.DataAnnotations;

namespace _2001_microservice.Models
{
    public class UserProfiles
    {
        [Key]
        public int ProfileId { get; set; }

        public int UserId { get; set; }
        public byte[]? ProfilePicture  { get; set; }
        public string? AboutMe { get; set; }
        public bool Archived { get; set; }
        public Users User { get; set; }

    }
}
