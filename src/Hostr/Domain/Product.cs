namespace Hostr.Domain;

public static class Product
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertProduct", "products");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateProduct", "products");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var p = new DB.Record();
        p.Set(cx.DB.ProductId, cx.DB.PoolIds.Next(cx.DBCx));
        p.Set(cx.DB.PoolName, name);
#pragma warning disable CS8629 
        p.Set(cx.DB.PoolCreatedBy, (DB.Record)cx.CurrentUser);
#pragma warning restore CS8629        
        return p;
    }
}