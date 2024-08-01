namespace Hostr.Web;

public abstract class Route
{
    public readonly Method Method;
    public readonly string Path;
    private readonly IEndpointFilter[] filters;

    public Route(Method method, string path, params IEndpointFilter[] filters)
    {
        Method = method;
        Path = path;
        this.filters = filters;
    }

    public RouteHandlerBuilder Bind(WebApplication app)
    {
        var rh = Method switch
        {
            Method.Get => app.MapGet(Path, (Delegate)Exec),
            Method.Post => app.MapPost(Path, (Delegate)Exec),
            _ => throw new Exception("Not implemented")
        };

        foreach (var f in filters) { rh.AddEndpointFilter(f); }
        return rh;
    }

    public abstract Task<object> Exec(HttpContext hcx);
}