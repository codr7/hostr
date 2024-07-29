using Hostr;

using DB = Hostr.DB;
using UI = Hostr.UI;

const int PASSWORD_ITERS = 10000;

var db = new Schema();
var cx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
cx.Connect();
var tx = cx.StartTx();

DB.Definition[] definitions = [db.UserIds, db.Users, db.PoolIds, db.Pools];
//foreach (var d in definitions.Reverse()) { d.DropIfExists(tx); }
var firstRun = !db.Users.Exists(tx);
foreach (var d in definitions) { d.Sync(tx); }

using var ui = new UI.Shell();

try
{
    ui.Say("Hostr v1");
    DB.Record? user = null;

    if (firstRun)
    {
        ui.Say("Fresh database detected, creating user");
        var name = ui.Ask("Name: ");
        if (name is null) { throw new Exception("Missing name"); }
        var email = ui.Ask("Email: ");
        if (email is null) { throw new Exception("Missing email"); }
        var password = ui.Ask("Password: ");
        if (password is null) { throw new Exception("Missing password"); }

        var hu = new DB.Record();
        hu.Set(db.UserId, 0);
        hu.Set(db.UserName, "hostr");
        hu.Set(db.UserEmail, "hostr");
        hu.Set(db.UserPassword, "");
        db.Users.Insert(hu, tx);

        var u = new DB.Record();
        u.Set(db.UserName, name);
        u.Set(db.UserEmail, email);
        u.Set(db.UserPassword, Password.Hash(password, PASSWORD_ITERS));
        u.Set(db.UserCreatedBy, hu);
        db.Users.Insert(u, tx);
        user = u;

        var p = new DB.Record();
        p.Set(db.PoolName, "double");
        p.Set(db.PoolCreatedBy, u);
        db.Pools.Insert(p, tx);        

        ui.Say("User successfully created");
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