using System.Threading;
using System.Threading.Tasks;
using OpenTracing;

namespace MoviesApi.Controllers;

public class RatingService
{
    private readonly ITracer _tracer;

    public RatingService(ITracer tracer)
    {
        _tracer = tracer;
    }

    public int GetRating(string movieName)
    {
        using var span = _tracer.ForComponent("ratings-service").WithTag("movieName", movieName).StartActive();
        var delay = movieName == "die hard 2" ? 3000 : 300;
        Thread.Sleep(delay);
        return 5;
    }
}