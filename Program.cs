using Hostr;
using Hostr.Domain;

using DB = Hostr.DB;
using Web = Hostr.Web;

var dbCx = new DB.Cx("localhost", "hostr", "hostr", "hostr");
dbCx.Connect();
var cx = new Cx(Schema.Instance, dbCx);
var tx = dbCx.StartTx();
cx.DB.DropIfExists(tx);
var firstRun = !cx.DB.Users.Exists(tx) || cx.DB.Users.Count(null, tx) == 0;
cx.DB.Sync(tx);

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
        cx.PostEvent(User.INSERT, null, ref hu, tx);
        Say("System user 'hostr' created");
        cx.Login(hu, tx);

        var u = User.Make(cx, name, email, password);
        u.Set(cx.DB.UserCreatedBy, hu);
        cx.PostEvent(User.INSERT, null, ref u, tx);
        cx.Login(u, tx);
        Say($"User '{name}' created");

        var makeTax = (string name, decimal percentage) =>
        {
            var tt = TaxType.Make(cx, name);
            cx.PostEvent(TaxType.INSERT, null, ref tt, tx);

            var tr = TaxRate.Make(cx, tt, percentage);
            cx.PostEvent(TaxRate.INSERT, null, ref tr, tx);

            return tt;
        };

        var tt = makeTax("VAT/Lodging", 12);
        makeTax("VAT/Food", 15);
        makeTax("VAT", 25);

        var r = Product.Make(cx, "double room");
        r.Set(cx.DB.ProductSalesTax, tt);
        cx.PostEvent(Product.INSERT, null, ref r, tx);

        var c = Charge.Make(cx, r, 100M, false);
        cx.PostEvent(Charge.INSERT, null, ref c, tx);

        r = Pool.Make(cx, "rooms");
        cx.PostEvent(Pool.INSERT, null, ref r, tx);

        r = Unit.Make(cx, "room 1");
        cx.PostEvent(Unit.INSERT, null, ref r, tx);

        r = Unit.Make(cx, "room 2");
        cx.PostEvent(Unit.INSERT, null, ref r, tx);

        r = Unit.Make(cx, "conf/S1");
        cx.PostEvent(Unit.INSERT, null, ref r, tx);

        r = Unit.Make(cx, "conf/S2");
        cx.PostEvent(Unit.INSERT, null, ref r, tx);

        r = Unit.Make(cx, "conf/L");
        cx.PostEvent(Unit.INSERT, null, ref r, tx);

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