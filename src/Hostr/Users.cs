using Hostr.DB;

namespace Hostr;

public static class Users
{

    public struct InsertUser : Events.Type
    {
        public void Exec(Schema db, Record evt, Record key, Record data, Tx tx)
        {
            db.Users.Insert(data, tx);
        }

        public string Id => "InsertUser";

        public Table Table(Schema db) => db.Users;
    }

    public static readonly InsertUser INSERT_USER = new InsertUser();

    public struct UpdateUser : Events.Type
    {
        public void Exec(Schema db, Record evt, Record key, Record data, Tx tx)
        {
            var rec = db.Users.Find(key, tx);
            
            if (rec is Record r)
            {
                db.Users.Update(r.Update(data), tx);
            }
            else
            {
                throw new Exception($"Record not found: {key}");
            }
        }

        public string Id => "UpdateUser";
        public Table Table(Schema db) => db.Users;
    }

    public static readonly UpdateUser UPDATE_USER = new UpdateUser();

    public const int PASSWORD_ITERS = 10000;

    public static DB.Record MakeUser(this Schema db, string name = "", string email = "", string password = "")
    {
        var u = new DB.Record();
        u.Set(db.UserName, name);
        u.Set(db.UserEmail, email);
        u.Set(db.UserPassword, (password == "") ? "" : Password.Hash(password, PASSWORD_ITERS));
        return u;
    }
}