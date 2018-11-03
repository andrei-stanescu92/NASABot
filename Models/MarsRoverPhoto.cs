using NASABot.Models;
using Newtonsoft.Json;

namespace NASABot.Models
{
    public class MarsRoverPhoto
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "img_src")]
        public string ImageSource { get; set; }

        public Rover Rover { get; set; }
    }
}