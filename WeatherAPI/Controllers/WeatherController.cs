using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        #region Base url and key
        const string OpenWeatherMapUrl = "https://api.openweathermap.org/data/2.5/weather";

        //This key is from "openweathermap.org/api"
        const string OpenWeatherMapKey = "a735d85aadc4cf277df601afba94ef24";

        //The second Url is from different provider (weatherapi.com), because it supports free key for history and forecast (30 days trial period)
        const string WeatherApiUrl = "http://api.weatherapi.com/v1";

        //This is the api key from different provider (weatherapi.com), because it has free key for history and forecast (30 days trial period)
        const string WeatherApiKey = "aad233da6a74474c88d75624231705";
        #endregion

        //Get current weather by location, with caching implementation
        [HttpGet("GetCurrentWeather")]
        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "location" })]
        public ActionResult<IEnumerable<WeatherData>> GetCurrentWeather(string location)
        {
            var apiUrl = $"{OpenWeatherMapUrl}?q={location}&appid={OpenWeatherMapKey}&units=metric";

            if (string.IsNullOrEmpty(location))
            {
                return BadRequest("The field for location are empty");
            }

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(apiUrl).Result;

                DateTime lastRefresh = DateTime.Now;

                var json = response.Content.ReadAsStringAsync().Result;
                var weatherData = JsonConvert.DeserializeObject<WeatherData>(json);

                if (!response.IsSuccessStatusCode)
                {
                    //return StatusCode((int)response.StatusCode, "Invalid location");
                    return BadRequest("Invalid location");
                }

                return Ok(weatherData);
            }
        }

        //Get forecast by location and number of days, with caching implementation
        [HttpGet("Forecast")]
        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "location", "days" })]
        public ActionResult<IEnumerable<WeatherData2>> GetForcast(string location, int days)
        {
            var apiUrl2 = $"{WeatherApiUrl}/forecast.json?key={WeatherApiKey}&q={location}&aqi=no&days={days}";
            WeatherData2 weatherData = new WeatherData2();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(apiUrl2).Result;

                string jsonString = response.Content.ReadAsStringAsync().Result;

                weatherData = JsonConvert.DeserializeObject<WeatherData2>(jsonString);

                if (string.IsNullOrEmpty(location) || days == 0)
                {
                    return BadRequest("The field for location or days are empty");
                }

                if (!response.IsSuccessStatusCode)
                {
                    //Helpful message
                    //return StatusCode((int)response.StatusCode);
                    return BadRequest("Invalid location or date.");
                }

                return Ok(weatherData);
            }
        }

        //Get weather history by location and date range(YYYY-MM-DD), with caching implementation
        [HttpGet("History")]
        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "location", "start_date", "end_date" })]
        public ActionResult<IEnumerable<WeatherData2>> GetWeatherHistory(string location, DateTime start_date, DateTime end_date)
        {
            var apiUrl2 = $"{WeatherApiUrl}/history.json?key={WeatherApiKey}&q={location}&aqi=no&dt={start_date}&end_dt={end_date}";
            WeatherData2 weatherData = new WeatherData2();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(apiUrl2).Result;

                string jsonString = response.Content.ReadAsStringAsync().Result;

                weatherData = JsonConvert.DeserializeObject<WeatherData2>(jsonString);

                if (string.IsNullOrEmpty(location) || start_date == null || end_date == null)
                {
                    return BadRequest("One or more fields are empty");
                }

                if (!response.IsSuccessStatusCode)
                {
                    //Helpful message
                    return BadRequest("Invalid location or date. Please check location input or date. Note for the date format(YYYY-MM-DD)");
                }

                return Ok(weatherData);
            }
        }
    }
}