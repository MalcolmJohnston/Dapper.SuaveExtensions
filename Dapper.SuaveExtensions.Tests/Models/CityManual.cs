using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    [Table("CitiesManual")]
    public class CityManual
    {
        [KeyType(KeyType.Assigned)]
        [Required]
        public string CityCode { get; set; }

        [Column("Name")]
        [Required]
        public string CityName { get; set; }
    }
}
