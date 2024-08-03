namespace Hostr.Web;

public static class RouteExtensions
{
    public static RouteHandlerBuilder Bind(this Route route, WebApplication app)
    {
        var p = $"/api/v1{route.Path}";

        var rh = route.Method switch
        {
            Method.Get => app.MapGet(p, (Delegate)route.Exec),
            Method.Post => app.MapPost(p, (Delegate)route.Exec),
            _ => throw new Exception("Not implemented")
        };

        if (route.Auth) { rh.RequireAuthorization(); }
        foreach (var f in route.Filters) { rh.AddEndpointFilter(f); }        
        return rh;
    }
}