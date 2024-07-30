using Hostr;

using DB = Hostr.DB;
using UI = Hostr.UI;

var db = new Schema();
var cx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
cx.Connect();
var tx = cx.StartTx();

DB.Definition[] definitions = [db.UserIds, db.Users, db.PoolIds, db.Pools, db.Units, db.Calendars];
//foreach (var d in definitions.Reverse()) { d.DropIfExists(tx); }
var firstRun = !db.Users.Exists(tx) || db.Users.Count(null, tx) == 0;
foreach (var d in definitions) { d.Sync(tx); }

using var ui = new UI.Shell();

try
{
    ui.Say("Hostr v1");
    DB.Record? user = null;

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
        db.Users.Insert(hu, tx);
        ui.Say("System user 'hostr' created");

        var u = db.MakeUser(name, email, password);
        u.Set(db.UserCreatedBy, hu);
        db.Users.Insert(u, tx);
        user = u;
        ui.Say($"User '{name}' created");

        var r = db.MakePool("double");
        r.Set(db.PoolCreatedBy, u);
        db.Pools.Insert(r, tx);

        r = db.MakeUnit("conf small");
        r.Set(db.UnitCreatedBy, u);
        db.Units.Insert(r, tx);

        ui.Say("Database seeded with examples");
    }
    else
    {
        tx.Commit();
        tx = cx.StartTx();
        ui.Say("Login");
        var id = ui.Ask("User: ");
        if (id is null) { throw new Exception("Missing user"); }
        var key = new DB.Record();
        key.Set(id.Contains('@') ? db.UserEmail : db.UserName, id);
        user = db.Users.Find(key, tx);

        if (user is DB.Record u)
        {
            var password = ui.Ask("Password: ");
            if (password is null) { throw new Exception("Missing password"); }

#pragma warning disable CS8604
            if (!Password.Check(u.Get(db.UserPassword), password)) { throw new Exception("Wrong password"); }
#pragma warning restore CS8604

            u.Set(db.UserLoginAt, DateTime.UtcNow);
            db.Users.Update(u, tx);
        }
        else
        {
            throw new Exception($"User not found: {id}");
        }
    }

    tx.Commit();
}
catch (Exception e)
{
    ui.Say(e);
}