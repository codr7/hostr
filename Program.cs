using Hostr;

using DB = Hostr.DB;
using UI = Hostr.UI;
using Web = Hostr.Web;

var dbCx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
dbCx.Connect();
var cx = new Cx(dbCx);
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

        var hu = cx.DB.MakeUser("hostr", "hostr");
        hu.Set(cx.DB.UserId, 0);
        cx.PostEvent(Users.INSERT, null, ref hu, tx);
        ui.Say("System user 'hostr' created");

        var u = cx.DB.MakeUser(name, email, password);
        u.Set(cx.DB.UserCreatedBy, hu);
        cx.PostEvent(Users.INSERT, null, ref u, tx);
        cx.Login(u, tx);
        ui.Say($"User '{name}' created");

        var r = cx.DB.MakePool("double");
        r.Set(cx.DB.PoolCreatedBy, u);
        cx.PostEvent(Pools.INSERT, null, ref r, tx);

        r = cx.DB.MakeUnit("conf small");
        r.Set(cx.DB.UnitCreatedBy, u);
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

    //Console.WriteLine("EVENT " + cx.DB.Events.FindFirst(null, tx));
    tx.Commit();
}
catch (Exception e)
{
    ui.Say(e);
}

var app = Web.App.Make(cx);
new Thread(() => app.Run()).Start();