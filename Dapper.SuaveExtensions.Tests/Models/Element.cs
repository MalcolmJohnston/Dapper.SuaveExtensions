// <copyright file="Itinerary.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    [Table("ElementTable")]
    public class Element
    {
        [KeyType(KeyType.Assigned)]
        public int BookingId { get; set; }

        [KeyType(KeyType.Assigned)]
        [Column("ItinId")]
        public int ItineraryId { get; set; }

        [KeyType(KeyType.Sequential)]
        public int ElementId { get; set; }

        [Required]
        [Column("Title")]
        public string ElementTitle { get; set; }
    }
}
