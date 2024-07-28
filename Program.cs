using Npgsql;
using Hostr;
using DB = Hostr.DB;

var users = new DB.Table("users");
var userName = new DB.Columns.Text(users, "name");
var userEmail = new DB.Columns.Text(users, "email")
{
    PrimaryKey = true
};

var admin = new DB.Record();
admin.Set(userName, "admin");
admin.Set(userEmail, "admin@admin.com"); 

await using var db = NpgsqlDataSource.Create("Host=localhost;Username=hostr;Password=hostr;Database=hostr");
await using var cmd = db.CreateCommand("SELECT email FROM users");

await using (var reader = await cmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        Console.WriteLine(reader.GetString(0));
    }
}
