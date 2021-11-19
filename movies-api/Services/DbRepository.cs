using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoviesApi.Models;
using OpenTracing;

namespace MoviesApi.Services;

public class DbRepository
{
    private readonly ITracer _tracer;

    static Dictionary<int, string> _movies = new()
    {
        { 1, "die hard" },
        { 2, "die hard 2" },
        { 3, "die hard with the vengeance" },
        { 4, "squid games" },
    };

    public DbRepository(ITracer tracer)
    {
        _tracer = tracer;
    }

    public List<Movie> FindMovies(string movieName)
    {
        using var scope = _tracer.ForDatabase("movies-db").WithTag("searchFilter", movieName).StartActive();
        var results = _movies.Where(x => x.Value.Contains(movieName)).Select(x => new Movie() { Id = x.Key, Name = x.Value }).ToList();
        if (results.Any(x => x.Name == "squid games")) // terrible
        {
            Thread.Sleep(2000);
        }

        return results;
    }
}