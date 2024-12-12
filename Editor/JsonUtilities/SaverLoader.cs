using System;
using System.Collections.Generic;
using System.IO;
using static JsonParser;
using UMLClassDiag;


public static class SaverLoader
{
    private static string generatedContentFolder = "generatedContent"; 
    private static string filePath = Path.Combine(generatedContentFolder, "currentUML.json");

    public static void SaveUML(string input)
    {
        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(generatedContentFolder))
            {
                Directory.CreateDirectory(generatedContentFolder);
            }

            File.WriteAllText(filePath, input);
            Console.WriteLine("UML saved successfully to " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving UML: " + ex.Message);
        }
    }

    public static void LoadUML()
    {
        try
        {
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine("UML file not found at " + filePath);
                return;
            }
            

            string jsonString = File.ReadAllText(filePath);
            Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
            var root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
            Console.WriteLine("UML loaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading UML: " + ex.Message);
        }
    }
}