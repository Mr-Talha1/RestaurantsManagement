using System.ComponentModel.DataAnnotations;

namespace TBAppBackend.Models
{
    public class City
    {
        [Key]
        public int ID { get; set; }
        public string? CityName { get; set; }
        public int? CountryID { get; set; }

    }
}
