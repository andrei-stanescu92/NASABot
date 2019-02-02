using NASABot.Models;
using Newtonsoft.Json.Linq;
using System;

namespace NASABot.Helpers
{
    public static class AsteroidHelper
    {
        public static string StartDate;

        public static void GetVelocity(Asteroid asteroid, JToken jToken)
        {
            var velocityJsonObject = jToken["close_approach_data"][0];
            var relativeVelocityData = velocityJsonObject["relative_velocity"];
            asteroid.Velocity = decimal.Parse(relativeVelocityData["miles_per_hour"].Value<string>());
        }

        public static void GetDiameter(Asteroid asteroid, JToken jToken)
        {
            var diameterJsonProperty = jToken["estimated_diameter"];
            asteroid.MinEstimatedDiameter = Decimal.Parse(diameterJsonProperty["miles"]["estimated_diameter_min"].Value<string>());
            asteroid.MaxEstimatedDiameter = decimal.Parse(diameterJsonProperty["miles"]["estimated_diameter_max"].Value<string>());
        }
    }
}
