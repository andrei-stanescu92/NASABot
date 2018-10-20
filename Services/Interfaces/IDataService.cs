using System.Collections.Generic;

namespace NASABot.Services
{
    public interface IDataService
    {
        object GetAsteroidInfo();
        List<object> GetAsteroids();
        object GetMarsRoverPhoto();
        object GetPictureOfTheDay();
    }
}