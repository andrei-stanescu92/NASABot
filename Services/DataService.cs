using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace NASABot.Services
{
    public class DataService : IDataService
    {
        private readonly string apiKey;

        public DataService(IConfiguration configuration)
        {
            apiKey = configuration["apiKey"];
        }

        public object GetPictureOfTheDay()
        {
            return new object();
        }

        public List<object> GetAsteroids()
        {
            return new List<object>();
        }

        public object GetAsteroidInfo()
        {
            return new object();
        }

        public object GetMarsRoverPhoto()
        {
            return new object();
        }
    }
}
