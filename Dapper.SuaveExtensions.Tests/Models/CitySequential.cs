using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    [Table("CitiesSequential")]
    public class CitySequential
    {
        [KeyType(KeyType.Sequential)]
        public short CityId { get; set; }

        [Required]
        public string CityCode { get; set; }

        [Column("Name")]
        [Required]
        public string CityName { get; set; }
    }
}
