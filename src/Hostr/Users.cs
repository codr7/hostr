namespace Hostr;

public static class Users
{
    public struct Insert : Events.Type
    {
        public void Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx)
        {
            cx.DB.Users.Insert(ref data, tx);
        }

        public string Id => "InsertUser";

        public DB.Table Table(Cx cx) => cx.DB.Users;
    }

    public static readonly Insert INSERT = new Insert();

    public struct Update : Events.Type
    {
        public void Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx)
        {
            if (key is null) { throw new Exception("Null user key"); }
            var rec = cx.DB.Users.Find((DB.Record)key, tx);
            
            if (rec is DB.Record r)
            {
                r.Update(data);
                cx.DB.Users.Update(ref data, tx);
            }
            else
            {
                throw new Exception($"User not found: {key}");
            }
        }

        public string Id => "UpdateUser";
        public DB.Table Table(Cx cx) => cx.DB.Users;
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