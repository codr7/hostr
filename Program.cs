using Hostr;
using Hostr.Domain;

using DB = Hostr.DB;
using Models = Hostr.Domain.Models;
using Web = Hostr.Web;

var dbCx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
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

        var hu = User.Make(cx, "hostr", "hostr");
        hu.Set(cx.DB.UserId, 0);
        cx.PostEvent(User.INSERT, null, ref hu); Say("System user 'hostr' created");
        cx.Login(hu);

        var u = User.Make(cx, name, email, password);
        u.Set(cx.DB.UserCreatedBy, hu);
        cx.PostEvent(User.INSERT, null, ref u);
        cx.Login(u);
        Say($"User '{name}' created");

        var makeTax = (string name, decimal percentage) =>
        {
            var tt = TaxType.Make(cx, name);
            cx.PostEvent(TaxType.INSERT, null, ref tt);

            var tr = TaxRate.Make(cx, tt, percentage);
            cx.PostEvent(TaxRate.INSERT, null, ref tr);

            return tt;
        };

        var tt = makeTax("VAT/Lodging", 12);
        makeTax("VAT/Food", 15);
        makeTax("VAT", 25);

        var r = Product.Make(cx, "double room");
        r.Set(cx.DB.ProductSalesTax, tt);
        cx.PostEvent(Product.INSERT, null, ref r);

        var c = Charge.Make(cx, u, r, 1000M, true);
        cx.PostEvent(Charge.INSERT, null, ref c);

        r = Pool.Make(cx, "rooms");
        cx.PostEvent(Pool.INSERT, null, ref r);

        r = Unit.Make(cx, "room 1");
        cx.PostEvent(Unit.INSERT, null, ref r);

        r = Unit.Make(cx, "room 2");
        cx.PostEvent(Unit.INSERT, null, ref r);

        r = Unit.Make(cx, "conf/S1");
        cx.PostEvent(Unit.INSERT, null, ref r);

        r = Unit.Make(cx, "conf/S2");
        cx.PostEvent(Unit.INSERT, null, ref r);

        r = Unit.Make(cx, "conf/L");
        cx.PostEvent(Unit.INSERT, null, ref r);

        Say("Database seeded with examples");
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