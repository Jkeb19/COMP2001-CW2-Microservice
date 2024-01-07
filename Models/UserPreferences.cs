using System.ComponentModel.DataAnnotations;

namespace _2001_microservice.Models
{
    public class UserPreferences
    {

        [Key]
        public int PrefID { get; set; }

        public int UserId { get; set; }
        public string UnitPreferences { get; set; }
        public string TimePreferences { get; set; }
        public string MarketingLanguage { get; set; }
        public Users User { get; set; }

    }
}
