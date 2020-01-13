// <copyright file="Itinerary.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    [Table("AssignedPairAndSequential")]
    public class AssignedPairAndSequential
    {
        [KeyType(KeyType.Assigned)]
        public int FirstAssignedId { get; set; }

        [KeyType(KeyType.Assigned)]
        public int SecondAssignedId { get; set; }

        [KeyType(KeyType.Sequential)]
        public int SequentialId { get; set; }

        [Required]
        [Column("Title")]
        public string Heading { get; set; }
    }
}
