using Hostr.DB;

namespace Hostr;

public static class Users
{

    public struct Insert : Events.Type
    {
        public void Exec(Cx cx, Record evt, Record? key, ref Record data, Tx tx)
        {
            cx.DB.Users.Insert(ref data, tx);
        }

        public string Id => "InsertUser";

        public Table Table(Cx cx) => cx.DB.Users;
    }

    public static readonly Insert INSERT = new Insert();

    public struct Update : Events.Type
    {
        public void Exec(Cx cx, Record evt, Record? key, ref Record data, Tx tx)
        {
            if (key is null) { throw new Exception("Null key"); }
            var rec = cx.DB.Users.Find((DB.Record)key, tx);
            
            if (rec is Record r)
            {
                r.Update(data);
                cx.DB.Users.Update(ref data, tx);
            }
            else
            {
                throw new Exception($"Record not found: {key}");
            }
        }

        public string Id => "UpdateUser";
        public Table Table(Cx cx) => cx.DB.Users;
    }

    public static readonly Update UPDATE = new Update();

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