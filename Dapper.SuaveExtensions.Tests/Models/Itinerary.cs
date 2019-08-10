// <copyright file="Itinerary.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.TotalCRUD.DataAnnotations;

namespace Dapper.TotalCRUD.Test.Models
{
    public class Itinerary
    {
        [DbKey(KeyType.Manual)]
        public int BookingId { get; set; }

        [DbKey(KeyType.Sequential)]
        public int ItineraryId { get; set; }

        [Required]
        [Column("Title")]
        public string ItineraryTitle { get; set; }
    }
}
