using Hostr;
using DB = Hostr.DB;
using UI = Hostr.UI;

const int PASSWORD_ITERATIONS = 10000;

var users = new DB.Table("users");
var userName = new DB.Columns.Text(users, "name");
var userEmail = new DB.Columns.Text(users, "email") { PrimaryKey = true };
var userPassword = new DB.Columns.Text(users, "password");
var userEmailKey = new DB.Key(users, "usersEmailKey", userEmail);

var cx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
cx.Connect();
var tx = cx.StartTx();
var firstRun = !users.Exists(tx);
users.Sync(tx);
using var ui = new UI.Shell();

try
{
    ui.Say("Hostr v1");
    DB.Record? user = null;

    if (firstRun)
    {
        ui.Say("Fresh database detected, setting up admin user");
        var name = ui.Ask("Name: ");
        if (name is null) { throw new Exception("Missing name"); }
        var email = ui.Ask("Email: ");
        if (email is null) { throw new Exception("Missing email"); }
        var password = ui.Ask("Password: ");
        if (password is null) { throw new Exception("Missing password"); }

        var u = new DB.Record();
        u.Set(userName, name);
        u.Set(userEmail, email);
        u.Set(userPassword, Password.Hash(password, PASSWORD_ITERATIONS));
        users.Insert(u, tx);
        user = u;
        ui.Say("Admin user successfully created");
    }
    else
    {
        tx.Commit();
        tx = cx.StartTx();
        ui.Say("Login");
        var id = ui.Ask("User: ");
        if (id is null) { throw new Exception("Missing user"); }
        var key = new DB.Record();

        if (id.Contains('@'))
        {
            key.Set(userEmail, id);
        }
        else
        {
            key.Set(userName, id);
        }

        user = users.Find(key, tx);

        if (user is DB.Record u)
        {
            var password = ui.Ask("Password: ");
            if (password is null) { throw new Exception("Missing password"); }
#pragma warning disable CS8604 
            if (!Password.Check(u.Get(userPassword), password)) { throw new Exception("Wrong password"); }
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