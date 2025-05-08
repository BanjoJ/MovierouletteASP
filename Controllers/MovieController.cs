using Microsoft.AspNetCore.Mvc;
using MovieRoulette.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace MovieRoulette.Controllers
{
    public class MovieController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public MovieController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _apiKey = configuration["OMDB:ApiKey"];
            _httpClient = httpClientFactory.CreateClient();
        }
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRandomMovie(string genre)
        {
            var searchUrl = $"http://www.omdbapi.com/?apikey={_apiKey}&s={genre}&type=movie";
            var searchResponse = await _httpClient.GetStringAsync(searchUrl);
            var searchJson = JsonDocument.Parse(searchResponse);

            if (!searchJson.RootElement.TryGetProperty("Search", out var movies))
            {
                return Json(new { error = "No movies found." });
            }

            var random = new Random().Next(movies.GetArrayLength());
            var selected = movies[random];
            var imdbID = selected.GetProperty("imdbID").GetString();

            var detailUrl = $"http://www.omdbapi.com/?apikey={_apiKey}&i={imdbID}";
            var detailResponse = await _httpClient.GetStringAsync(detailUrl);
            var movie = JsonSerializer.Deserialize<Movie>(detailResponse);

            return Json(movie);
        }
    }
}
