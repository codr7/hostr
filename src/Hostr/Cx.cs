using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Hostr.Domain;
using static Hostr.DB.ValueExtensions;

namespace Hostr;

public class Cx
{ 
    public Cx(Schema db, DB.Cx dbCx)
    {
        DB = db;
        DBCx = dbCx;
        Json = new Json(db);
        JwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("eyJhbGciOiJIUzI1NiJ9.ew0KICAic3ViIjogIjEyMzQ1Njc4OTAiLA0KICAibmFtZSI6ICJBbmlzaCBOYXRoIiwNCiAgImlhdCI6IDE1MTYyMzkwMjINCn0.KXlzwhGodgi8yqntLOHggIpvnElHeVImJYNro1NQX00"));
    }

    public DB.Record? CurrentUser => currentUser;
    public readonly Schema DB;
   public readonly DB.Cx DBCx;
    public readonly Json Json;
    public readonly SymmetricSecurityKey JwtKey;

    public void Login(DB.Record user, DB.Tx tx)
    {
        currentUser = user;
        user.Set(DB.UserLoginAt, DateTime.UtcNow);
        PostEvent(User.UPDATE, user.Copy(DB.Users.PrimaryKey.Columns), ref user, tx);
    }

    public DB.Record Login(long userId, DB.Tx tx)
    {
        if (DB.Users.FindFirst(DB.UserId.Eq(userId), tx) is DB.Record u) {
            currentUser = u;    
            return u;
        }

        throw new Exception($"User not found: {userId}");
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

    public void PostEvent(Event.Type type, DB.Record? key, ref DB.Record data, DB.Tx tx)
    {
        var e = new DB.Record();
        e.Set(DB.EventId, DB.EventIds.Next(tx));
        e.Set(DB.EventType, type.Id);
        e.Set(DB.EventPostedAt, DateTime.UtcNow);
        if (key != null) { e.Set(DB.EventKey, JsonDocument.Parse(Json.ToString(key))); }
        if (currentUser is DB.Record u) { e.Set(DB.EventPostedBy, u); }
        if (currentEvents.Count > 0) { currentEvents.Last().Copy(ref e, DB.Events.PrimaryKey.Columns.Zip(DB.EventParent.Columns).ToArray()); }
        currentEvents.Push(e);

        try
        {
            var d = type.Exec(this, e, key, ref data, tx);
            e.Set(DB.EventData, JsonDocument.Parse(Json.ToString(d)));

            for (var i = 0; i < currentEvents.Count; i++)
            {
                var ce = currentEvents[i];

                if (ce.Id == e.Id)
                {
                    DB.Events.Store(ref ce, this, tx);
                }
                else if (!DB.Events.Stored(ce, tx))
                {
                    DB.Events.Insert(ref ce, this, tx);
                }

                currentEvents[i] = ce;
            };
        }
        finally
        {
            if (!currentEvents.Pop().Equals(e)) { throw new Exception("Event popped out of order"); }
        }
    }

    private List<DB.Record> currentEvents = new List<DB.Record>();
    private DB.Record? currentUser;
}