using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoviesApi.Models;
using MoviesApi.Services;
using OpenTracing;
using OpenTracing.Tag;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesController> _logger;
        private readonly DbRepository _dbRepository;
        private readonly RatingService _ratingsService;
        public MoviesController(ILogger<MoviesController> logger, DbRepository dbRepository, RatingService ratingsService)
        {
            _dbRepository = dbRepository;
            _ratingsService = ratingsService;
            _logger = logger;

        }

        [HttpGet]
        public List<MovieSearchResult> Get(string movieName)
        {
            var movies = _dbRepository.FindMovies(movieName);
            var searchResults = movies.Select(x => new MovieSearchResult() { Id = x.Id, Name = x.Name }).ToList();
            foreach (var movie in searchResults)
            {
                var rating =  _ratingsService.GetRating(movie.Name);
                movie.Rating = rating;
            }

            return searchResults;

        }

        public class MovieSearchResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Rating { get; set; }
        }
    }
}

