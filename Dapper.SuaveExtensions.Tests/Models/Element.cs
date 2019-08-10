// <copyright file="Itinerary.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.TotalCRUD.DataAnnotations;

namespace Dapper.TotalCRUD.Test.Models
{
    [Table("ElementTable")]
    public class Element
    {
        [DbKey(KeyType.Manual, Order = 1)]
        public int BookingId { get; set; }

        [DbKey(KeyType.Manual, Order = 2)]
        [Column("ItinId")]
        public int ItineraryId { get; set; }

        [DbKey(KeyType.Sequential, Order = 3)]
        public int ElementId { get; set; }

        [Required]
        [Column("Title")]
        public string ElementTitle { get; set; }
    }
}
