// <copyright file="Itinerary.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Tests.Models
{
    public class Itinerary
    {
        [KeyType(KeyType.Assigned)]
        public int BookingId { get; set; }

        [KeyType(KeyType.Sequential)]
        public int ItineraryId { get; set; }

        [Required]
        [Column("Title")]
        public string ItineraryTitle { get; set; }
    }
}
