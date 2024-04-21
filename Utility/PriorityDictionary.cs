namespace SilhouetteDance.Utility;

/// <summary>
/// Priority dictionary, extends <see cref="Dictionary{TKey,TValue}"/> and adds a priority to each key-value pair.
/// Matches the key-value pair with the highest priority. (lower number = higher priority)
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TPriority"></typeparam>
public class PriorityDictionary<TKey, TValue, TPriority> : Dictionary<TKey, TValue> 
    where TKey : notnull 
    where TValue : notnull 
    where TPriority : IComparable<TPriority>
{
    private readonly SortedDictionary<TPriority, List<TKey>> _priorityDictionary = new();
    
    public TValue this[(TKey, TPriority) key]
    {
        set => SetValue(key.Item1, value, key.Item2);
    }
    
    public new TValue this[TKey key] => GetValue(key);

    public TValue GetValue(TKey key)
    {
        foreach (var (_, keys) in _priorityDictionary)
        {
            foreach (var k in keys)
            {
                if (k.Equals(key)) return base[k];
            }
        }
        throw new KeyNotFoundException();
    }
    
    public TValue StartwithPriority(TKey pattern, out TKey matched)
    {
        if (pattern is not string) throw new InvalidOperationException("Only string key can use this method.");

        foreach (var (_, keys) in _priorityDictionary)
        {
            foreach (var k in keys)
            {
                if (pattern.ToString()?.StartsWith(k.ToString() ?? string.Empty) == true)
                {
                    matched = k;
                    return base[k];
                }
            }
        }
        throw new KeyNotFoundException();
    }

    public void SetValue(TKey key, TValue value, TPriority priority)
    {
        bool priorityExists = _priorityDictionary.TryGetValue(priority, out var keys);
        if (priorityExists)
        {
            if (keys?.Contains(key) == true) keys.Remove(key);
            keys?.Add(key);
        }
        else _priorityDictionary.Add(priority, new List<TKey> {key});
        
        base[key] = value;
    }

    public TPriority GetPriority(TKey key)
    {
        foreach (var (priority, keys) in _priorityDictionary)
        {
            foreach (var k in keys)
            {
                if (k.Equals(key)) return priority;
            }
        }
        throw new KeyNotFoundException();
    }
}