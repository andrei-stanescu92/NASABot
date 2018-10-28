using NASABot.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NASABot.Services.Interfaces
{
    public interface IDataService
    {
        Task<List<Asteroid>> GetAsteroids(string startDate, string endDate);
        Task<List<MarsRoverPhoto>> GetMarsRoverPhoto(string earthDate);
        Task<PictureOfTheDay> GetCurrentPictureOfTheDay();
    }
}