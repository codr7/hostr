using Hostr;
using Hostr.Domain;

using DB = Hostr.DB;
using UI = Hostr.UI;
using Web = Hostr.Web;

var dbCx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
dbCx.Connect();
var cx = new Cx(Schema.Instance, dbCx);
var tx = dbCx.StartTx();
cx.DB.DropIfExists(tx);
var firstRun = !cx.DB.Users.Exists(tx) || cx.DB.Users.Count(null, tx) == 0;
cx.DB.Sync(tx);

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

        var hu = User.Make(cx, "hostr", "hostr");
        hu.Set(cx.DB.UserId, 0);
        cx.PostEvent(User.INSERT, null, ref hu, tx);
        ui.Say("System user 'hostr' created");
        cx.Login(hu, tx);

        var u = User.Make(cx, name, email, password);
        u.Set(cx.DB.UserCreatedBy, hu);
        cx.PostEvent(User.INSERT, null, ref u, tx);
        cx.Login(u, tx);
        ui.Say($"User '{name}' created");

        var r = User.Make(cx, "double");
        r.Set(cx.DB.PoolCreatedBy, u);
        cx.PostEvent(Pool.INSERT, null, ref r, tx);

        r = Unit.Make(cx, "conf small");
        r.Set(cx.DB.UnitCreatedBy, u);
        cx.PostEvent(Unit.INSERT, null, ref r, tx);

        ui.Say("Database seeded with examples");
    }

    tx.Commit();
}
catch (Exception e)
{
    ui.Say(e);
    ui.Flush();
    Environment.Exit(-1);
}

var tx1 = dbCx.StartTx();
var tx2 = dbCx.StartTx();
tx2.Rollback();
tx1.Commit();

ui.Flush();
var app = Web.App.Make(cx);
app.Run();