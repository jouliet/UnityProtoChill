using System;
using System.Collections.Generic;
using System.Text;

public static class JsonParser
{
    public static object Parse(string json)
    {
        // Remove whitespaces and make the JSON easier to work with
        json = json.Trim();
        
        if (json.StartsWith("{"))
        {
            return ParseObject(ref json);
        }
        else if (json.StartsWith("["))
        {
            return ParseArray(ref json);
        }
        else
        {
            throw new Exception("Invalid JSON format");
        }
    }

    // Parse a JSON object (starts with { and ends with })
    private static Dictionary<string, object> ParseObject(ref string json)
    {
        var obj = new Dictionary<string, object>();
        json = json.Substring(1).Trim(); // Remove the opening '{' and trim
        while (!json.StartsWith("}"))
        {
            // Parse the key
            string key = ParseString(ref json);
            json = json.Substring(1).Trim(); // Remove the colon and trim

            // Parse the value
            object value = ParseValue(ref json);

            // Add key-value pair to dictionary
            obj[key] = value;

            // Check if there's another key-value pair or if the object is ending
            if (json.StartsWith(","))
            {
                json = json.Substring(1).Trim(); // Skip the comma
            }
        }
        json = json.Substring(1).Trim(); // Remove the closing '}'
        return obj;
    }

    // Parse a JSON array (starts with [ and ends with ])
    private static List<object> ParseArray(ref string json)
    {
        var array = new List<object>();
        json = json.Substring(1).Trim(); // Remove the opening '[' and trim
        while (!json.StartsWith("]"))
        {
            // Parse the value
            object value = ParseValue(ref json);
            array.Add(value);

            // Check if there's another value or if the array is ending
            if (json.StartsWith(","))
            {
                json = json.Substring(1).Trim(); // Skip the comma
            }
        }
        json = json.Substring(1).Trim(); // Remove the closing ']'
        return array;
    }

    // Parse a string value in JSON (enclosed in double quotes)
    private static string ParseString(ref string json)
    {
        // Remove the opening quote
        json = json.Substring(1);
        int closingQuoteIndex = json.IndexOf('"');
        string result = json.Substring(0, closingQuoteIndex);
        json = json.Substring(closingQuoteIndex + 1).Trim(); // Remove the closing quote
        return result;
    }

    // Parse a value, which can be an object, array, string, number, true, false, or null
    private static object ParseValue(ref string json)
    {
        json = json.Trim();
        if (json.StartsWith("{"))
        {
            return ParseObject(ref json);
        }
        else if (json.StartsWith("["))
        {
            return ParseArray(ref json);
        }
        else if (json.StartsWith("\""))
        {
            return ParseString(ref json);
        }
        else if (json.StartsWith("true") || json.StartsWith("false"))
        {
            bool boolValue = json.StartsWith("true");
            json = json.Substring(boolValue ? 4 : 5).Trim();
            return boolValue;
        }
        else if (json.StartsWith("null"))
        {
            json = json.Substring(4).Trim();
            return null;
        }
        else
        {
            return ParseNumber(ref json);
        }
    }

    // Parse a number (either integer or float)
    private static object ParseNumber(ref string json)
    {
        int endIndex = 0;
        while (endIndex < json.Length && (char.IsDigit(json[endIndex]) || json[endIndex] == '.' || json[endIndex] == '-'))
        {
            endIndex++;
        }

        string numberString = json.Substring(0, endIndex).Trim();
        json = json.Substring(endIndex).Trim();

        if (numberString.Contains("."))
        {
            return double.Parse(numberString); // Parse float
        }
        else
        {
            return int.Parse(numberString); // Parse int
        }
    }


public static string ObjectToString(object obj)
{
    UnityEngine.Debug.Log("o m g");
    if (obj is Dictionary<string, object> dict)
    {
        var result = new StringBuilder();
        foreach (var keyValue in dict)
        {
            result.AppendLine($"{keyValue.Key}:");
            result.AppendLine(ObjectToString(keyValue.Value)); // Recursively call for value
        }
        return result.ToString();
    }
    else if (obj is List<object> list)
    {
        var result = new StringBuilder();
        foreach (var item in list)
        {
            result.AppendLine(ObjectToString(item)); // Recursively call for item
        }
        return result.ToString();
    }
    else
    {
        return obj?.ToString() ?? "null"; // Return the object as string, handle null case
    }
}

}