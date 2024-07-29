namespace Hostr;

public static class User {
    public const int PASSWORD_ITERS = 10000;

    public static DB.Record MakeUser(this Schema db, string name = "", string email = "", string password = "") {
        var u = new DB.Record();
        u.Set(db.UserName, name);
        u.Set(db.UserEmail, email);
        u.Set(db.UserPassword, (password == "") ? "" : Password.Hash(password, PASSWORD_ITERS));
        return u;
    }
}