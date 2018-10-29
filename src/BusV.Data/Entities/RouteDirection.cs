using System.ComponentModel.DataAnnotations;

namespace BusV.Data.Entities
{
    public class RouteDirection
    {
        [Required]
        public string Tag { get; set; }

        [Required]
        public string Title { get; set; }

        public string Name { get; set; }

        [Required]
        public string[] Stops { get; set; }
    }
}
