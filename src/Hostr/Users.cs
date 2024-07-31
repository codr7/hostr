using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
            var rec = cx.DB.Users.FindFirst((DB.Record)key, tx);

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

    public static string MakeJwtToken(Cx cx, DB.Record user)
    {
        var creds = new SigningCredentials(
                    cx.JwtKey,
                    SecurityAlgorithms.HmacSha256);

        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim("id", $"{user.Get(cx.DB.UserId)}"));
#pragma warning disable CS8604
        claims.AddClaim(new Claim("displayName", user.Get(cx.DB.UserDisplayName)));
        claims.AddClaim(new Claim(ClaimTypes.Email, user.Get(cx.DB.UserEmail)));
#pragma warning restore CS8604
        claims.AddClaim(new Claim(ClaimTypes.Role, "admin"));

        var td = new SecurityTokenDescriptor
        {
            SigningCredentials = creds,
            Expires = DateTime.UtcNow.AddHours(24),
            Subject = claims
        };

        var h = new JwtSecurityTokenHandler();
        var t = h.CreateToken(td);
        return h.WriteToken(t);
    }

    public static DB.Record MakeUser(this Schema db, string name = "", string email = "", string password = "")
    {
        var u = new DB.Record();
        u.Set(db.UserDisplayName, name);
        u.Set(db.UserEmail, email);
        u.Set(db.UserPassword, (password == "") ? "" : Password.Hash(password, PASSWORD_ITERS));
        return u;
    }
}