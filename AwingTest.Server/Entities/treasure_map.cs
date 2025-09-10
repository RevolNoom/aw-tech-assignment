using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwingTest.Server.Entities
{
    [Table("treasure_map")]
    public class treasure_map
    {
        [Key]
        public int? id { get; set; }

        public int? rows { get; set; }
        
        public int? columns { get; set; }
        
        public int? chest_types { get; set; }
        
        public double? min_total_distance { get; set; }
        
        public string? map_offset_str { get; set; }

        public string? path_x_offset_str { get; set; }
        
        public string? path_y_offset_str { get; set; }

        [NotMapped]
        public List<List<int?>>? map_client_input { get; set; }

        [NotMapped]
        public List<List<int>>? map_offset { get; set; }

        [NotMapped]
        public List<int>? path_x { get; set; }

        [NotMapped]
        public List<int>? path_y { get; set; }

        [NotMapped]
        public List<int>? path_x_offset { get; set; }

        [NotMapped]
        public List<int>? path_y_offset { get; set; }
    }
}
