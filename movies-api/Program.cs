using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MoviesApi;
using MoviesApi.Controllers;
using MoviesApi.Services;
using Steeltoe.Extensions.Configuration.Placeholder;
using Wavefront.AspNetCore.SDK.CSharp.Common;
using Wavefront.OpenTracing.SDK.CSharp;
using Wavefront.OpenTracing.SDK.CSharp.Reporting;
using Wavefront.SDK.CSharp.Common;
using Wavefront.SDK.CSharp.Common.Application;
using Wavefront.SDK.CSharp.DirectIngestion;

var builder = WebApplication.CreateBuilder(args);
((IConfigurationBuilder)builder.Configuration).Sources.Clear();
builder.Configuration
    .AddYamlFile("appsettings.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", optional: true, reloadOnChange: true)
    .AddYamlFile("appsettings.user.yaml", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .AddPlaceholderResolver();

var wavefrontOptions = new WavefrontOptions();
builder.Configuration.Bind("Wavefront", wavefrontOptions);

var services = builder.Services;

IWavefrontSender wavefrontSender = new WavefrontDirectIngestionClient.Builder(wavefrontOptions.Client.Server, wavefrontOptions.Client.Token).Build();
var applicationTags = new ApplicationTags.Builder(wavefrontOptions.Tags.ApplicationName, wavefrontOptions.Tags.Service)
    .Cluster(wavefrontOptions.Tags.Cluster)
    .Shard(wavefrontOptions.Tags.Shard)
    .CustomTags(wavefrontOptions.Tags.CustomTags)
    .Build();
    
var wavefrontSpanReporter =  new WavefrontSpanReporter.Builder()
    .WithSource(wavefrontOptions.Source)
    .Build(wavefrontSender);
var consoleReporter = new ConsoleReporter(wavefrontOptions.Source);
var compositeReporter = new CompositeReporter(wavefrontSpanReporter, consoleReporter);
var wfAspNetCoreReporter = new WavefrontAspNetCoreReporter.Builder(applicationTags)
    .WithSource(wavefrontOptions.Source)
    .Build(wavefrontSender);

var tracer = new WavefrontTracer.Builder(compositeReporter, applicationTags).Build();
services.AddControllers();
services.AddWavefrontForMvc(wfAspNetCoreReporter, tracer);
services.AddScoped<DbRepository>();
services.AddScoped<RatingService>();
services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo {Title = "MyAPI", Version = "v1"}));


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyAPI v1");
});
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

namespace MoviesApi
{
    public class WavefrontSenderConfig
    {
        public string Server { get; set; }
        public string Token { get; set; }
    }

    public class ApplicationTagsConfig
    {
        public string ApplicationName { get; set; }
        public string Cluster { get; set; }
        public string Service { get; set; }
        public string Shard { get; set; }
        public Dictionary<string, string> CustomTags { get; set; } = new();
    }

    public class WavefrontOptions
    {
        public string Source { get; set; } = Dns.GetHostName();
        public WavefrontSenderConfig Client { get; set; }
        public ApplicationTagsConfig Tags { get; set; }
    }
}