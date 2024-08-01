using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Hostr;

public static class Users
{
    public static Events.Type INSERT => new Events.Insert("InsertUser", "users");
    public static Events.Type UPDATE => new Events.Update("UpdateUser", "users");

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