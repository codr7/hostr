using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Hostr.Domain.Models;

public class User : Model
{
    public static Event.Type INSERT => new Event.Insert("InsertUser", "users");
    public static Event.Type UPDATE => new Event.Update("UpdateUser", "users");
    public static readonly int PASSWORD_ITERS = 10000;
    public static readonly string JWT_ISSUER = "hostr";

    public static TokenValidationParameters GetJwtValidationParameters(Cx cx) => new TokenValidationParameters
    {
        IssuerSigningKey = cx.JwtKey,
        ValidateIssuer = true,
        ValidIssuer = JWT_ISSUER,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidAudience = ""
    };

    public static long ValidateJwtToken(Cx cx, string token)
    {
        var ps = GetJwtValidationParameters(cx);
        var h = new JwtSecurityTokenHandler();
        SecurityToken st;
        h.ValidateToken(token[7..], ps, out st);
#pragma warning disable CS8602 
        var p = (st as JwtSecurityToken).Payload;
#pragma warning restore CS8602 
        object? v;
        p.TryGetValue("userId", out v);
#pragma warning disable CS8600
#pragma warning disable CS8604 
        var id = long.Parse((string)v);
#pragma warning restore CS8604
#pragma warning restore CS8600
        return id;
    }

    public User(Cx cx, DB.Record fields) : base(cx, fields) { }

    public User(Cx cx, string name = "", string email = "", string password = "") : base(cx)
    {
        Record.Set(cx.DB.UserId, cx.DB.UserIds.Next(cx.DBCx));
        if (cx.CurrentUser is User cu) { Record.Set(cx.DB.UserCreatedBy, cu.Record); }
        DisplayName = name;
        Email = email;
        Password = password;
    }

    public bool CheckPassword(string password) => Hostr.Password.Check(Password, password);

    public long Id => Record.Get(Cx.DB.UserId);

    public string DisplayName
    {
        get => Record.Get(Cx.DB.UserDisplayName)!;
        set => Record.Set(Cx.DB.UserDisplayName, value);
    }

    public string Email
    {
        get => Record.Get(Cx.DB.UserEmail)!;
        set => Record.Set(Cx.DB.UserEmail, value);
    }
    public DateTime LoginAt
    {
        get => Record.Get(Cx.DB.UserLoginAt);
        set => Record.Set(Cx.DB.UserLoginAt, value);
    }

    public string MakeJwtToken(Cx cx)
    {
        var creds = new SigningCredentials(
                    cx.JwtKey,
                    SecurityAlgorithms.HmacSha256);

        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim("userId", $"{Id}"));
        claims.AddClaim(new Claim("displayName", DisplayName));
        claims.AddClaim(new Claim(ClaimTypes.Email, Email));
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
    public string Password
    {
        get => Record.Get(Cx.DB.UserPassword)!;
        set => Record.Set(Cx.DB.UserPassword, (value == "") ? "" : Hostr.Password.Hash(value, PASSWORD_ITERS));
    }

    public override DB.Table[] Tables => [Cx.DB.Users];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}