using DB = Hostr.DB;
using UI = Hostr.UI;

var users = new DB.Table("users");
var userName = new DB.Columns.Text(users, "name");
var userEmail = new DB.Columns.Text(users, "email")
{
    PrimaryKey = true
};
var userPassword = new DB.Columns.Text(users, "password");

var cx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
cx.Connect();
var tx = cx.StartTx();
var firstRun = !users.Exists(tx);
users.Sync(tx);

var ui = new UI.Shell();
ui.Say("Hostr v1");
ui.Say("Fresh database detected, setting up admin user...");

if (firstRun)
{
    var name = ui.Ask("Display Name: ");
    if (name == null) { throw new Exception("Missing name"); }
    var email = ui.Ask("Email: ");
    if (email == null) { throw new Exception("Missing email"); }
    var password = ui.Ask("Password: ");
   if (password == null) { throw new Exception("Missing password"); }

    var admin = new DB.Record();
    admin.Set(userName, name);
    admin.Set(userEmail, email);
    admin.Set(userPassword, password);
    users.Insert(admin, tx);
}

tx.Commit();
ui.Reset();