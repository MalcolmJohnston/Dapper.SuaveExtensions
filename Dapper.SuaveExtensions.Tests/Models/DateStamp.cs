using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    [Table("DateStampTest")]
    public class DateStamp
    {
        [KeyType(KeyType.Assigned)]
        public string Name { get; set; }

        public string Value { get; set; }

        [DateStamp]
        [ReadOnly(true)]
        public DateTime InsertDate { get; set; }

        [DateStamp]
        public DateTime UpdateDate { get; set; }
    }
}
