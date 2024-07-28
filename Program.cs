using DB = Hostr.DB;
using Hostr.DB;

var users = new DB.Table("users");
var userName = new DB.Columns.Text(users, "name");
var userEmail = new DB.Columns.Text(users, "email")
{
    PrimaryKey = true
};

var admin = new DB.Record();
admin.Set(userName, "admin");
admin.Set(userEmail, "admin@admin.com"); 

var cx = new Cx("localhost", "hostr", "hostr", "hostr");
cx.Connect();
var tx = cx.StartTx();
if (!users.Exists(tx)) { users.Create(tx); }

tx.Commit();