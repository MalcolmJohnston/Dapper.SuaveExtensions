using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.TotalCRUD.DataAnnotations;

namespace Dapper.TotalCRUD.Test.Models
{
    [Table("DateStampTest")]
    public class DateStamp
    {
        [DbKey(KeyType.Manual)]
        public string Name { get; set; }

        public string Value { get; set; }

        [DateStamp]
        [ReadOnly(true)]
        public DateTime InsertDate { get; set; }

        [DateStamp]
        public DateTime UpdateDate { get; set; }
    }
}
