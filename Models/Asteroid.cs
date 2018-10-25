using Newtonsoft.Json;
using System.Collections.Generic;

namespace NASABot.Services
{
    public class Asteroid
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(PropertyName = "is_potentially_hazardous_asteroid")]
        public bool IsPotentiallyHazardous { get; set; }
    }
}
