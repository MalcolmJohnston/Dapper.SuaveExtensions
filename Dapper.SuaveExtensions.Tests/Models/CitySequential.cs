using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.TotalCRUD.DataAnnotations;

namespace Dapper.TotalCRUD.Test.Models
{
    [Table("CitiesSequential")]
    public class CitySequential
    {
        [DbKey(KeyType.Sequential)]
        public short CityId { get; set; }

        [Required]
        public string CityCode { get; set; }

        [Column("Name")]
        [Required]
        public string CityName { get; set; }
    }
}
