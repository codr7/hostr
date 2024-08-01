namespace Hostr.Web;

public interface Route
{
    IEndpointFilter[] Filters { get; }
    Method Method { get; }
    string Path { get; }

    Task<object> Exec(HttpContext hcx);
}