namespace Hostr.DB;

public static class ValueExtensions
{
    public static Condition Eq(this Value left, object right) => new Condition($"{left.ValueSql} = $?", [right]);
    public static Condition Gt(this Value left, object right) => new Condition($"{left.ValueSql} > $?", [right]);
    public static Condition Gte(this Value left, object right) => new Condition($"{left.ValueSql} >= $?", [right]);
    public static Condition Lt(this Value left, object right) => new Condition($"{left.ValueSql} < $?", [right]);
    public static Condition Lte(this Value left, object right) => new Condition($"{left.ValueSql} <= $?", [right]);
}