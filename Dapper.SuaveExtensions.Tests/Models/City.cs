using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    [Table("Cities")]
    public class City
    {
        [Key]
        public int CityId { get; set; }

        [Required]
        public string CityCode { get; set; }

        [Column("Name")]
        [Required]
        public string CityName { get; set; }

        [Required]
        public string Area { get; set; }
    }
}
