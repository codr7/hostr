namespace Hostr.DB;

using RecordId = ulong;

public class ValueStore {
    private readonly Dictionary<(RecordId, Column), object> storedValues = new Dictionary<(RecordId, Column), object>();
    
    public object? GetStoredValue(RecordId recId, Column col) {
        object? v = null;
        storedValues.TryGetValue((recId, col), out v);
        return v;
    }

    public void StoreValue(RecordId recId, Column col, object val) => storedValues[(recId, col)] = val;
}