using System.ComponentModel.DataAnnotations;

namespace _2001_microservice.Models
{
    public class UserTrails
    {
        public int UserId { get; set; }

        [Key]
        public int TrailId { get; set; }


    }
}
