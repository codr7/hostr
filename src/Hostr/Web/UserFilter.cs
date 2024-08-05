using Hostr.Domain;

namespace Hostr.Web;

public struct UserFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.Items["cx"] is Cx cx)
        {
            var req = context.HttpContext.Request;
            req.Headers.TryGetValue("Authorization", out var auth);
            var userId = User.ValidateJwtToken(cx, auth!);
            using var tx = cx.DBCx.StartTx();
            cx.Login(userId, tx);
            tx.Commit();
        }

        return next(context);
    }
}