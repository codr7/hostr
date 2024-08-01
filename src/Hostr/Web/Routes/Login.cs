namespace Hostr.Web;

using System.Text.Json.Serialization;

public static class Login
{
    public class Data
    {
        [JsonRequired] public string email { get; set; }
        [JsonRequired] public string password { get; set; }
    }

    async public static Task<object> Handler(HttpContext http)
    {
        Cx cx = new Cx(new Schema(), new DB.Cx("", "", "", "")); //TEMPORARY
        var request = http.Request;
        var stream = new StreamReader(request.Body);
        var body = await stream.ReadToEndAsync();
        var data = cx.Json.FromString<Data>(body)!;
        using var tx = cx.DBCx.StartTx();
        var u = cx.Login(data.email, data.password, tx);
        return Users.MakeJwtToken(cx, u);
    }
}