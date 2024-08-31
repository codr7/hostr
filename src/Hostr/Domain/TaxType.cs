using static Hostr.DB.ValueExtensions;

namespace Hostr.Domain;

public static class TaxType
{
    public static readonly Event.Type INSERT = new Event.Insert("Insert Tax Type", "taxTypes");
    public static readonly Event.Type UPDATE = new Event.Update("Update Tax Type", "taxTypes");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var t = new DB.Record();
        t.Set(cx.DB.TaxTypeName, name);
        return t;
    }
}