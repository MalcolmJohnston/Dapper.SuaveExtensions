using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    public class ReadOnly
    {
        [KeyType(KeyType.Sequential)]
        public short SequentialId { get; set; }

        public string Editable { get; set; }

        [ReadOnly(true)]
        [Column("ReadOnly")]
        public string ReadOnlyProperty { get; set; }
    }
}
