namespace Hostr.Web;

public struct CxFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var dbCx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
        dbCx.Connect();
        context.HttpContext.Items["cx"] = new Cx(Schema.Instance, dbCx);
        return next(context);
    }
}