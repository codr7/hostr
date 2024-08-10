namespace Hostr.Domain;

public static class TaxType
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertTaxType", "taxTypes");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateTaxType", "taxTypes");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var t = new DB.Record();
        t.Set(cx.DB.TaxTypeName, name);
        return t;
    }
}