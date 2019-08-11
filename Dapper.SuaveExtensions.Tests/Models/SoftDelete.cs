using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    [Table("SoftDeleteTest", Schema = "Suave")]
    public class SoftDelete
    {
        [KeyType(KeyType.Identity)]
        public int SoftDeleteId { get; set; }

        [SoftDelete(1, 0)]
        public int RecordStatus { get; set; }
    }
}
