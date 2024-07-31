namespace Hostr;

using System.Text.Json;

public class Cx
{
    public Cx(Schema db, DB.Cx dbCx)
    {
        DB = db;
        DBCx = dbCx;
        Json = new Json(db);
    }

    public Schema DB;
    public DB.Cx DBCx;
    public readonly Json Json;

    public void PostEvent(Events.Type type, DB.Record? key, ref DB.Record data, DB.Tx tx)
    {
        var e = new DB.Record();
        e.Set(DB.EventId, DB.EventIds.Next(tx));
        e.Set(DB.EventType, type.Id);
        e.Set(DB.EventPostedAt, DateTime.UtcNow);
        if (currentUser is DB.Record u) { e.Set(DB.EventPostedBy, u); }
        if (currentEvents.Count > 0) { currentEvents.Last().Copy(ref e, DB.Events.PrimaryKey.Columns.Zip(DB.EventParent.ForeignColumns).ToArray()); }
        currentEvents.Push(e);
        
        try {
            type.Exec(this, e, key, ref data, tx);
            if (key != null) { e.Set(DB.EventKey, JsonDocument.Parse(Json.ToString(key))); }
            e.Set(DB.EventData, JsonDocument.Parse(Json.ToString(data)));
            DB.Events.Insert(ref e, tx);
        } finally {
            if (!currentEvents.Pop().Equals(e)) { throw new Exception("Event popped out of order"); }
        }
    }

    public void Login(DB.Record user, DB.Tx tx)
    {
        user.Set(DB.UserLoginAt, DateTime.UtcNow);
        PostEvent(Users.UPDATE, user.Copy(DB.Users.PrimaryKey.Columns), ref user, tx);
        this.currentUser = user;
    }

    private List<DB.Record> currentEvents = new List<DB.Record>();
    private DB.Record? currentUser;
}