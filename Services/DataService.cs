using Microsoft.Extensions.Configuration;
using NASABot.Helpers;
using NASABot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NASABot.Services
{
    public class DataService : IDataService
    {
        private readonly string apiKey;
        private const string baseUrl = "https://api.nasa.gov/";
        private HttpClient client;

        public DataService(IConfiguration configuration)
        {
            apiKey = configuration["apiKey"];
            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<PictureOfTheDay> GetCurrentPictureOfTheDay()
        {
            HttpResponseMessage responseMessage = await this.client.GetAsync(baseUrl + $"planetary/apod?api_key={apiKey}");
            PictureOfTheDay pictureOfTheDay = null;
            if (responseMessage.IsSuccessStatusCode)
            {
                pictureOfTheDay = await responseMessage.Content.ReadAsAsync<PictureOfTheDay>();
            }
            return pictureOfTheDay;
        }

        public async Task<List<Asteroid>> GetAsteroids(string startDate, string endDate)
        {
            HttpResponseMessage httpResponseMessage = await this.client.GetAsync(baseUrl + $"neo/rest/v1/feed?start_date={startDate}&end_date={endDate}&api_key={apiKey}");
            List<Asteroid> asteroids = new List<Asteroid>();
            if(httpResponseMessage.IsSuccessStatusCode)
            {
                var responseMessageContent = await httpResponseMessage.Content.ReadAsStringAsync();

                var data = (JObject)JsonConvert.DeserializeObject(responseMessageContent);

                var nearEarthObjectsData = data["near_earth_objects"];

                foreach(var child in nearEarthObjectsData.Children())
                {
                    List<JToken> jArray = child.Values().ToList();

                    foreach (var jToken in jArray)
                    {
                        Asteroid asteroidObject = JsonConvert.DeserializeObject<Asteroid>(jToken.ToString());

                        AsteroidHelper.GetDiameter(asteroidObject, jToken);
                        AsteroidHelper.GetVelocity(asteroidObject, jToken);

                        asteroids.Add(asteroidObject);
                    }
                }
            }

            return asteroids;
        }

        public async Task<List<MarsRoverPhoto>> GetMarsRoverPhoto(string earthDate)
        {
            HttpResponseMessage httpResponseMessage = await this.client.GetAsync(baseUrl + $"mars-photos/api/v1/rovers/curiosity/photos?earth_date={earthDate}&api_key={this.apiKey}");
            List<MarsRoverPhoto> marsRoverPhotos = new List<MarsRoverPhoto>();
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

                var photosJsonObject = (JObject)JsonConvert.DeserializeObject(responseContent);

                List<JToken> listOfPhotos = photosJsonObject["photos"].Children().ToList();
                // loop through json object and retrieve each photo
                foreach (JToken photo in listOfPhotos)
                {
                    marsRoverPhotos.Add(photo.ToObject<MarsRoverPhoto>());
                }
            }

            return marsRoverPhotos;
        }
    }
}
