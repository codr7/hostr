namespace Hostr.Web;

public class CxFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var db = new Schema();
        var dbCx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
        dbCx.Connect();
        context.HttpContext.Items["cx"] = new Cx(db, dbCx);
        return next(context);
    }
}