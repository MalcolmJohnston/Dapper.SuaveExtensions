using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper.TotalCRUD.DataAnnotations;

namespace Dapper.TotalCRUD.Test.Models
{
    [Table("SoftDeleteTest", Schema = "holy")]
    public class SoftDelete
    {
        [DbKey(KeyType.Identity)]
        public int SoftDeleteId { get; set; }

        [SoftDelete(1, 0)]
        public int RecordStatus { get; set; }
    }
}
