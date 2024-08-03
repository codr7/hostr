namespace Hostr.Web;

public interface Route
{
    bool Auth => false;
    IEndpointFilter[] Filters { get; }
    Method Method { get; }
    string Path { get; }

    Task<object> Exec(HttpContext hcx);
}