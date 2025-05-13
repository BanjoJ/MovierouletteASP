using Microsoft.AspNetCore.Mvc;
using MovieRoulette.Models;
using System.Text.Json;


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
        public async Task<IActionResult> GetRandomMovies(string genre)
        {
            var searchUrl = $"http://www.omdbapi.com/?apikey={_apiKey}&s={genre}&type=movie";
            var searchResponse = await _httpClient.GetStringAsync(searchUrl);
            var searchJson = JsonDocument.Parse(searchResponse);

            if (!searchJson.RootElement.TryGetProperty("Search", out var movies) || movies.GetArrayLength() < 3)
            {
                return Json(new { error = "Not enough movies found." });
            }

            var random = new Random();
            var movieList = movies.EnumerateArray().ToList();
            int n = movieList.Count;

            // API palauttaa ennakoitavat tulokset, joten luodaan satunnaisuutta
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = movieList[i];
                movieList[i] = movieList[j];
                movieList[j] = temp;
            }

            var selectedMovies = new List<string>();
            for (int i = 0; i < 3 && i < movieList.Count; i++)
            {
                selectedMovies.Add(movieList[i].GetProperty("imdbID").GetString());
            }

            var finalMovies = new List<Movie>();

            foreach (var imdbID in selectedMovies)
            {
                var detailUrl = $"http://www.omdbapi.com/?apikey={_apiKey}&i={imdbID}";
                var detailResponse = await _httpClient.GetStringAsync(detailUrl);
                var movie = JsonSerializer.Deserialize<Movie>(detailResponse);
                if (movie != null)
                    finalMovies.Add(movie);
            }

            return Json(finalMovies);
        }
    }
}
