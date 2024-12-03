using UnityEditor;
using UnityEngine;
using System.Text.Json;
using System.Text.Json.Serialization;
using static JsonParser;
using System.Collections.Generic;


public class UMLDiag : GenerativeProcess
{
    public UMLDiag()
    {
        // UML est abonné si tout se passe bien
        Debug.Log("UMLDiag process initialized.");
    }

    public override void OnSubmit(string input)
    {
        Debug.Log("Submit received in UMLDiag. Generating UML...");
        GenerateUML(input);
    }

    private void GenerateUML(string input)
    {
        string jsonString = GPTGenerator.GenerateUML(input);
        Debug.Log("Generated UML JSON: " + jsonString);

        //Le cast est nécessaire

        Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
        JSONMapper.MapToBaseObject(parsedObject);
        Debug.Log("ParsedObject : "+ ObjectToString(parsedObject));
    }
}
