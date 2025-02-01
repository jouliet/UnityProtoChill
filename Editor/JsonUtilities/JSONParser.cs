using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UMLClassDiag;


public static class JsonParser
{
    
    public static object Parse(string json)
    {
        if (string.IsNullOrEmpty(json)){
            throw new Exception("Json is null so it can't be parsed.");
        }
        
        json = json.Trim();

        // Supprimer les lignes avant celle qui commence par ```
        while (!json.StartsWith("```")){
            if (json.StartsWith("{"))
            {
                break;
            }

            // Trouver l'index de la première occurrence d'un retour à la ligne
            int newlineIndex = json.IndexOf('\n');
            
            // Si aucun retour à la ligne n'est trouvé, on quitte (sinon boucle infinie)
            if (newlineIndex == -1)
            {
                break;
            }

            // Supprimer la première ligne en tronquant la chaîne après le retour à la ligne
            json = json.Substring(newlineIndex + 1);
        }
        if (json.StartsWith("```")) {
            string[] lignes = json.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            json = string.Join("\n", lignes, 1, lignes.Length - 1);
        }
        string jsonMarker = "```json";



        //Detecter le marker pour complètement ignorer les comms random
        int jsonStartIndex = json.IndexOf(jsonMarker);
        if (jsonStartIndex != -1)
        {
            json = json.Substring(jsonStartIndex + jsonMarker.Length).Trim();  // Extract only the JSON part
        }

       
    
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
        if (closingQuoteIndex == -1)
        {
            throw new Exception("Invalid JSON format: Unterminated string");
        }
        string result = json.Substring(0, closingQuoteIndex);
        json = json.Substring(closingQuoteIndex + 1).Trim(); // Remove the closing quote
        return result;
    }

    // Types : object, array, string, bool or null. TODO? numbers et vecteurs il y a des pistes mais est ce même utile.
    // Il manque le cas des nombres et vecteurs (liste de nombres ou objet unity?) que gpt aime bien formatter (0,0,0) mais c'est surtout la "default value" qui pose problème actuellement de ce pdv la et c'est pas dit
    // Qu'on ce soit la responsabilité du parser de s'occuper de ça. A voir comment ça avance vis à vis de la scène. La gestion comme ça (avec des "default values" pour handle level design et les positions
    // en faisant d'une pierre 2 coups) semble être un sacré flop.
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
            
            return ParseString(ref json);
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
    if (obj is Dictionary<string, object> dict)
    {
        var result = new StringBuilder();
        foreach (var keyValue in dict)
        {
            result.AppendLine($"{keyValue.Key}:");
            result.AppendLine(ObjectToString(keyValue.Value)); 
        }
        return result.ToString();
    }
    else if (obj is List<object> list)
    {
        var result = new StringBuilder();
        foreach (var item in list)
        {
            result.AppendLine(ObjectToString(item)); 
        }
        return result.ToString();
    }
    else
    {
        return obj?.ToString() ?? "null"; 
    }
}

public static string GetUserResponse(string response){

    if (string.IsNullOrEmpty(response))
    {
        throw new Exception("Response is null or empty so it can't be parsed.");
    }

    response = response.Trim();
    string userMarker = "```user";

    int userStartIndex = response.IndexOf(userMarker);
    if (userStartIndex != -1)
    {
        response = response.Substring(userStartIndex + userMarker.Length).Trim();
    }
    int endMarkerIndex = response.IndexOf("```");
        if (endMarkerIndex != -1)
        {
            response = response.Substring(0, endMarkerIndex).Trim();
        }
    return response;
    }
public static string GetScript(string response){
    return response;
}


}

