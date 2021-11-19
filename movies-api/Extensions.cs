using System.Diagnostics;
using OpenTracing;
using OpenTracing.Tag;

namespace MoviesApi;

public static class Extensions
{
    public static ISpanBuilder ForDatabase(this ITracer tracer, string dbName)
    {
        var caller = new StackTrace().GetFrames()[1].GetMethod();
        return tracer.BuildSpan($"{caller.DeclaringType.Name}.{caller.Name}")
            .AddReference(References.FollowsFrom, tracer.ActiveSpan.Context)
            .WithTag(Tags.SpanKind, Tags.SpanKindClient)
            .WithTag(Tags.Component, "java-jdbc")
            .WithTag(Tags.DbInstance, dbName)
            .WithTag(Tags.DbType, "sql");
    }
    public static ISpanBuilder ForComponent(this ITracer tracer, string component)
    {
        var caller = new StackTrace().GetFrames()[1].GetMethod();
        return tracer.BuildSpan($"{caller.DeclaringType.Name}.{caller.Name}")
            .AddReference(References.FollowsFrom, tracer.ActiveSpan.Context)
            .WithTag(Tags.SpanKind, Tags.SpanKindClient)
            .WithTag(Tags.Component, component);
    }
}