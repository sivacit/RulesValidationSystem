using System.Dynamic;

public static class Utils
{

/// <summary>
/// Converts a given object into a flat <see cref="Dictionary{TKey, TValue}"/> of string keys and values,
/// where nested properties are represented with dot notation in the keys. Special handling is applied for
/// ExpandoObject, IDictionary, and IEnumerable of ExpandoObject. If the prefix is "Fields", it is dropped
/// from the resulting key.
/// </summary>
/// <param name="obj">
/// The object to flatten. Can be an <see cref="ExpandoObject"/>, <see cref="IDictionary{String, String}"/>,
/// <see cref="IEnumerable{ExpandoObject}"/>, or any other object.
/// </param>
/// <param name="prefix">
/// The prefix to prepend to each key in the resulting dictionary. Used for recursion; defaults to an empty string.
/// </param>
/// <returns>
/// A flat <see cref="Dictionary{String, String}"/> representing the object's properties and values.
/// </returns>
/// <example>
/// <code>
/// dynamic obj = new ExpandoObject();
/// obj.Name = "John";
/// obj.Address = new ExpandoObject();
/// obj.Address.City = "New York";
/// obj.Address.Zip = "10001";
/// var flatDict = Utils.ToFlatDictionary(obj);
/// // flatDict will contain:
/// // "Name" -> "John"
/// // "Address.City" -> "New York"
/// // "Address.Zip" -> "10001"
/// </code>
/// </example>
    public static Dictionary<string, string> ToFlatDictionary(object obj, string prefix = "")
    {
        var result = new Dictionary<string, string>();

        if (obj is ExpandoObject expando)
        {
            foreach (var kvp in (IDictionary<string, object>)expando)
            {
                // If prefix is "Fields", drop it
                string newPrefix = (prefix == "Fields") ? kvp.Key :
                                   string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

                foreach (var nested in ToFlatDictionary(kvp.Value, newPrefix))
                {
                    result[nested.Key] = nested.Value;
                }
            }
        }
        else if (obj is IDictionary<string, string> dict)
        {
            foreach (var kvp in dict)
            {
                string key = (prefix == "Fields") ? kvp.Key :
                             string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                result[key] = kvp.Value;
            }
        }
        else if (obj is IEnumerable<ExpandoObject> list)
        {
            int i = 0;
            foreach (var item in list)
            {
                foreach (var nested in ToFlatDictionary(item, $"{prefix}[{i}]"))
                {
                    result[nested.Key] = nested.Value;
                }
                i++;
            }
        }
        else
        {
            result[prefix] = obj?.ToString() ?? "";
        }
        return result;
    }
}
