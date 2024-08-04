using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Hostr;

public static class Users
{
    public static Events.Type INSERT => new Events.Insert("InsertUser", "users");
    public static Events.Type UPDATE => new Events.Update("UpdateUser", "users");

    public const int PASSWORD_ITERS = 10000;

    public static readonly string JWT_ISSUER = "hostr";

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
            Subject = claims,
            Issuer = JWT_ISSUER
        };

        var h = new JwtSecurityTokenHandler();
        
        var t = h.CreateJwtSecurityToken(td);
        return h.WriteToken(t);
    }

   public static long ValidateJwtToken(Cx cx, string token)
    {
        var ps = new TokenValidationParameters();
        ps.IssuerSigningKey = cx.JwtKey;
        ps.ValidateIssuer = true;
        ps.ValidIssuer = JWT_ISSUER;
        ps.ValidateLifetime = true;
        ps.ValidateIssuerSigningKey = true;
        ps.ValidateAudience = false;
        ps.ValidAudience = "";

        var h = new JwtSecurityTokenHandler();
        SecurityToken st;
        h.ValidateToken(token[7..], ps, out st);
#pragma warning disable CS8602 
        var p = (st as JwtSecurityToken).Payload;
#pragma warning restore CS8602 
        object v;
        p.TryGetValue("id", out v);
#pragma warning disable CS8600
#pragma warning disable CS8604 
        var id = long.Parse((string)v);
#pragma warning restore CS8604
#pragma warning restore CS8600
        return id;
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