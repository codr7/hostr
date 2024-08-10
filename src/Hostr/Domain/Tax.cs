using static Hostr.DB.ValueExtensions;

namespace Hostr.Domain;

public static class Tax
{
    public static decimal Calculate(Cx cx, DB.Record taxType, DateTime timestamp, decimal netAmount) => 
        TaxRate.Get(cx, taxType, timestamp) * netAmount;
}