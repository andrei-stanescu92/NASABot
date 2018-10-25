using System;

namespace NASABot.Models
{
    public class PictureOfTheDay
    {
        public string Copyright { get; set; }
        public DateTime Date { get; set; }
        public string Explanation { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}