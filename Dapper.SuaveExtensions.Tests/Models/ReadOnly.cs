using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.TotalCRUD.DataAnnotations;

namespace Dapper.TotalCRUD.Test.Models
{
    public class ReadOnly
    {
        [DbKey(KeyType.Sequential)]
        public short SequentialId { get; set; }

        public string Editable { get; set; }

        [ReadOnly(true)]
        [Column("ReadOnly")]
        public string ReadOnlyProperty { get; set; }
    }
}
