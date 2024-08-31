namespace Hostr.Domain;

public static class Product
{
    public static readonly Event.Type INSERT = new Event.Insert("Insert Product", "products");
    public static readonly Event.Type UPDATE = new Event.Update("Update Product", "products");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var p = new DB.Record();
        p.Set(cx.DB.ProductId, cx.DB.PoolIds.Next(cx.DBCx));
        p.Set(cx.DB.PoolName, name);
        p.Set(cx.DB.PoolCreatedBy, cx.CurrentUser!.Record);
        return p;
    }
}