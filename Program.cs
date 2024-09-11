using Hostr;
using Hostr.Domain;
using Hostr.Domain.Models;
using DB = Hostr.DB;
using Web = Hostr.Web;

var dbCx = new DB.Cx("/var/run/postgresql", "hostr", "hostr", "hostr");
dbCx.Connect();
var cx = new Cx(Schema.Instance, dbCx);
var tx = dbCx.StartTx();
cx.DB.DropIfExists(dbCx);
var firstRun = !cx.DB.Users.Exists(dbCx) || cx.DB.Users.Count(null, dbCx) == 0;
cx.DB.Sync(dbCx);

void Say(string what) => Console.WriteLine(what);

string? Ask(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine();
}

try
{
    Say("Hostr v1");

    if (firstRun)
    {
        Say("Setup User");
        var name = Ask("Name: ");
        if (name is null) { throw new Exception("Missing name"); }
        var email = Ask("Email: ");
        if (email is null) { throw new Exception("Missing email"); }
        var password = Ask("Password: ");
        if (password is null) { throw new Exception("Missing password"); }

        var hu = new User(cx, id: 0, name: "hostr", email: "hostr").Store();
        Say("System user 'hostr' created");
        cx.Login((User)hu);
        
        var u = new User(cx, name: name, email: email, password: password).Store();
        cx.Login((User)u);
        Say($"User '{name}' created");

        SeedDemoData();
    }

    tx.Commit();
}
catch (Exception e)
{
    Say(e.ToString());
    Environment.Exit(-1);
}

var app = Web.App.Make(cx);
app.Run();

TaxType MakeTax(string name, decimal percentage)
{
    var tt = new TaxType(cx, name: name);
    tt.Store();
    new TaxRate(cx, tt, percentage).Store();
    return tt;
};

void SeedDemoData()
{
    var tt = MakeTax("VAT/Lodging", 12);
    MakeTax("VAT/Food", 15);
    MakeTax("VAT", 25);

    var p = new Product(cx, name: "double room", salesTax: tt);
    p.Store();

    var c = Charge.Make(cx, cx.CurrentUser!.Record, p, 1000M, true);
    cx.PostEvent(Charge.INSERT, null, ref c);

    new Pool(cx, name: "rooms").Store();
    new Unit(cx, name: "room 1").Store();
    new Unit(cx, name: "room 2").Store();
    new Unit(cx, name: "conf part 1").Store();
    new Unit(cx, name: "conf part 2").Store();
    new Unit(cx, name: "conf whole").Store();

    Say("Database seeded with examples");
}