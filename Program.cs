using System.Text.Json.Serialization;
using Hostr;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DB = Hostr.DB;
using UI = Hostr.UI;
using Web = Hostr.Web;

var db = new Schema();
var dbCx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
dbCx.Connect();
var cx = new Cx(db, dbCx);
var tx = dbCx.StartTx();
db.DropIfExists(tx);
var firstRun = !db.Users.Exists(tx) || db.Users.Count(null, tx) == 0;
db.Sync(tx);

using var ui = new UI.Shell();

try
{
    ui.Say("Hostr v1");

    if (firstRun)
    {
        ui.Say("Setup User");
        var name = ui.Ask("Name: ");
        if (name is null) { throw new Exception("Missing name"); }
        var email = ui.Ask("Email: ");
        if (email is null) { throw new Exception("Missing email"); }
        var password = ui.Ask("Password: ");
        if (password is null) { throw new Exception("Missing password"); }

        var hu = db.MakeUser("hostr", "hostr");
        hu.Set(db.UserId, 0);
        cx.PostEvent(Users.INSERT, null, ref hu, tx);
        ui.Say("System user 'hostr' created");

        var u = db.MakeUser(name, email, password);
        u.Set(db.UserCreatedBy, hu);
        cx.PostEvent(Users.INSERT, null, ref u, tx);
        cx.Login(u, tx);
        ui.Say($"User '{name}' created");

        var r = db.MakePool("double");
        r.Set(db.PoolCreatedBy, u);
        cx.PostEvent(Pools.INSERT, null, ref r, tx);

        r = db.MakeUnit("conf small");
        r.Set(db.UnitCreatedBy, u);
        cx.PostEvent(Units.INSERT, null, ref r, tx);

        ui.Say("Database seeded with examples");
    }
    else
    {
        tx.Commit();
        tx = dbCx.StartTx();
        ui.Say("Login");
        var email = ui.Ask("Email: ");
        if (email is null) { throw new Exception("Missing email"); }
        var password = ui.Ask("Password: ");
        if (password is null) { throw new Exception("Missing password"); }
        cx.Login(email, password, tx);
    }

    Console.WriteLine("EVENT " + db.Events.FindFirst(null, tx));
    tx.Commit();
}
catch (Exception e)
{
    ui.Say(e);
}

WebApplication MakeApp()
{
    var builder = WebApplication.CreateBuilder();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(options =>
     {
         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             IssuerSigningKey = cx.JwtKey
         };
     });

    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    var app = builder.Build();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/ping", () => "pong");

    app.MapPost("login", (Delegate)Web.Login.Handler);
 
    app.MapPost("/stop", () => app.StopAsync()).
      RequireAuthorization();

    return app;
}

var app = MakeApp();
new Thread(() => app.Run()).Start();