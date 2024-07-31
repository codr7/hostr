namespace Hostr;

public static class Stack
{
    public static void Push<T>(this List<T> lst, T val) => lst.Add(val);
    
    public static T Pop<T>(this List<T> lst) {
        var i = lst.Count-1;
        var v = lst[i];
        lst.RemoveAt(lst.Count-1);
        return v;
    }
}