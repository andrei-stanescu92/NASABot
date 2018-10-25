using Newtonsoft.Json;
using System;

namespace NASABot.Models
{
    public class Rover
    {
        public string Name { get; set; }

        [JsonProperty(PropertyName = "landing_date")]
        public DateTime LandingDate { get; set; }

        [JsonProperty(PropertyName = "launch_date")]
        public DateTime LaunchDate { get; set; }

        [JsonProperty(PropertyName = "total_photos")]
        public int NumberOfPhotos { get; set; }
    }
}