namespace Hostr.Web;

public static class RouteExtensions
{
    public static RouteHandlerBuilder Bind(this Route route, WebApplication app)
    {
        var rh = route.Method switch
        {
            Method.Get => app.MapGet(route.Path, (Delegate)route.Exec),
            Method.Post => app.MapPost(route.Path, (Delegate)route.Exec),
            _ => throw new Exception("Not implemented")
        };

        foreach (var f in route.Filters) { rh.AddEndpointFilter(f); }
        return rh;
    }
}