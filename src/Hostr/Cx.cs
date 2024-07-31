namespace Hostr;

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

public class Cx
{
    public Cx(Schema db, DB.Cx dbCx)
    {
        DB = db;
        DBCx = dbCx;
        Json = new Json(db);
        JwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("eyJhbGciOiJIUzI1NiJ9.ew0KICAic3ViIjogIjEyMzQ1Njc4OTAiLA0KICAibmFtZSI6ICJBbmlzaCBOYXRoIiwNCiAgImlhdCI6IDE1MTYyMzkwMjINCn0.KXlzwhGodgi8yqntLOHggIpvnElHeVImJYNro1NQX00"));
    }

    public readonly Schema DB;
    public readonly DB.Cx DBCx;
    public readonly Json Json;
    public readonly SymmetricSecurityKey JwtKey;

    public void PostEvent(Events.Type type, DB.Record? key, ref DB.Record data, DB.Tx tx)
    {
        var e = new DB.Record();
        e.Set(DB.EventId, DB.EventIds.Next(tx));
        e.Set(DB.EventType, type.Id);
        e.Set(DB.EventPostedAt, DateTime.UtcNow);
        if (currentUser is DB.Record u) { e.Set(DB.EventPostedBy, u); }
        if (currentEvents.Count > 0) { currentEvents.Last().Copy(ref e, DB.Events.PrimaryKey.Columns.Zip(DB.EventParent.ForeignColumns).ToArray()); }
        currentEvents.Push(e);

        try
        {
            type.Exec(this, e, key, ref data, tx);
            if (key != null) { e.Set(DB.EventKey, JsonDocument.Parse(Json.ToString(key))); }
            e.Set(DB.EventData, JsonDocument.Parse(Json.ToString(data)));
            DB.Events.Insert(ref e, tx);
        }
        finally
        {
            if (!currentEvents.Pop().Equals(e)) { throw new Exception("Event popped out of order"); }
        }
    }

    public void Login(DB.Record user, DB.Tx tx)
    {
        user.Set(DB.UserLoginAt, DateTime.UtcNow);
        PostEvent(Users.UPDATE, user.Copy(DB.Users.PrimaryKey.Columns), ref user, tx);
        currentUser = user;
    }

    public DB.Record Login(string email, string password, DB.Tx tx)
    {
        if (DB.Users.FindFirst(DB.UserEmail.Eq(email), tx) is DB.Record u)
        {
#pragma warning disable CS8604
            if (!Password.Check(u.Get(DB.UserPassword), password)) { throw new Exception("Wrong password"); }
#pragma warning restore CS8604
            Login(u, tx);
            return u;
        }
        else
        {
            throw new Exception($"User not found: {email}");
        }
    }

    private List<DB.Record> currentEvents = new List<DB.Record>();
    private DB.Record? currentUser;
}