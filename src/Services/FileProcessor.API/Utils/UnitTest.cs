using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

public class UtilsTest
{
    [Fact]
    public void ToFlatDictionary_SimpleExpandoObject_ReturnsFlatDictionary()
    {
        dynamic obj = new ExpandoObject();
        obj.Name = "John";
        obj.Age = 30;

        var result = Utils.ToFlatDictionary(obj);

        Assert.Equal(2, result.Count);
        Assert.Equal("John", result["Name"]);
        Assert.Equal("30", result["Age"]);
    }

    [Fact]
    public void ToFlatDictionary_NestedExpandoObject_ReturnsFlatDictionaryWithDotNotation()
    {
        dynamic obj = new ExpandoObject();
        obj.Name = "John";
        obj.Address = new ExpandoObject();
        obj.Address.City = "New York";
        obj.Address.Zip = "10001";

        var result = Utils.ToFlatDictionary(obj);

        Assert.Equal(3, result.Count);
        Assert.Equal("John", result["Name"]);
        Assert.Equal("New York", result["Address.City"]);
        Assert.Equal("10001", result["Address.Zip"]);
    }

    [Fact]
    public void ToFlatDictionary_ExpandoObjectWithFieldsPrefix_DropsFieldsPrefix()
    {
        dynamic obj = new ExpandoObject();
        obj.Fields = new ExpandoObject();
        obj.Fields.FirstName = "Alice";
        obj.Fields.LastName = "Smith";

        var result = Utils.ToFlatDictionary(obj);

        Assert.Equal("Alice", result["FirstName"]);
        Assert.Equal("Smith", result["LastName"]);
    }

    [Fact]
    public void ToFlatDictionary_DictionaryInput_ReturnsFlatDictionary()
    {
        var dict = new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };

        var result = Utils.ToFlatDictionary(dict);

        Assert.Equal(2, result.Count);
        Assert.Equal("Value1", result["Key1"]);
        Assert.Equal("Value2", result["Key2"]);
    }

    [Fact]
    public void ToFlatDictionary_IEnumerableOfExpandoObject_ReturnsIndexedKeys()
    {
        var list = new List<ExpandoObject>();

        dynamic obj1 = new ExpandoObject();
        obj1.Name = "John";
        dynamic obj2 = new ExpandoObject();
        obj2.Name = "Jane";

        list.Add(obj1);
        list.Add(obj2);

        var result = Utils.ToFlatDictionary(list);

        Assert.Equal("John", result["[0].Name"]);
        Assert.Equal("Jane", result["[1].Name"]);
    }

    [Fact]
    public void ToFlatDictionary_PrimitiveValue_ReturnsDictionaryWithPrefixAsKey()
    {
        var result = Utils.ToFlatDictionary(42, "Number");

        Assert.Single(result);
        Assert.Equal("42", result["Number"]);
    }

    [Fact]
    public void ToFlatDictionary_NullValue_ReturnsEmptyString()
    {
        var result = Utils.ToFlatDictionary(null, "NullKey");

        Assert.Single(result);
        Assert.Equal("", result["NullKey"]);
    }
}