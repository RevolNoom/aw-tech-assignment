using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwingTest.Server.Entities
{
    [Table("weather_forecast")]
    public class WeatherForecast
    {
        [Key]
        public int id { get; set; }

        public DateOnly date { get; set; }

        public int temperature_c { get; set; }

        public int temperature_f => 32 + (int)(temperature_c / 0.5556);

        [StringLength(256)]
        public string? summary { get; set; }
    }
}
