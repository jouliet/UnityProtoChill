using System;
using System.Collections.Generic;
using System.IO;
using static JsonParser;
using UMLClassDiag;
using UnityPusher;
using UnityEngine;
using UnityEditor;


public static class SaverLoader
{
    private static string generatedContentFolder = "generatedContent"; 
    private static string UMLFilePath = Path.Combine(generatedContentFolder, "currentUML.json");
    private static string GOJsonFilePath = Path.Combine(generatedContentFolder, "currentGameObjects.json");

    public static void SaveUML(string input)
    {
        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(generatedContentFolder))
            {
                Directory.CreateDirectory(generatedContentFolder);
            }

            File.WriteAllText(UMLFilePath, input);
            Console.WriteLine("UML saved successfully to " + UMLFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving UML: " + ex.Message);
        }
    }

    public static void SaveGOJson(string input)
    {
        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(generatedContentFolder))
            {
                Directory.CreateDirectory(generatedContentFolder);
            }

            File.WriteAllText(GOJsonFilePath, input);
            Console.WriteLine("UML saved successfully to " + GOJsonFilePath);
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
            
            if (!File.Exists(UMLFilePath))
            {
                Console.WriteLine("UML file not found at " + UMLFilePath);
                return;
            }
            

            string jsonString = File.ReadAllText(UMLFilePath);
            GenerativeProcess.SetJsonScripts(jsonString);
            Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
            var root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
            Console.WriteLine("UML loaded");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading UML: " + ex.Message);
            Console.WriteLine("Error loading UML: " + ex.Message);
        }
    }

    public static void LoadGOJson()
    {
        try
        {
            
            if (!File.Exists(GOJsonFilePath))
            {
                Console.WriteLine("UML file not found at " + GOJsonFilePath);
                Debug.LogError("UML file not found at " + GOJsonFilePath);
                return;
            }
            

            string jsonString = File.ReadAllText(GOJsonFilePath);
            GenerativeProcess.SetJsonGOs(jsonString);
            GameObjectCreator.JsonToDictionary(jsonString);
            GameObjectCreator.StockEveryGOsInList();
            Console.WriteLine("UML loaded");
            Debug.Log("GOJson Loaded");
            //
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading GOJson: " + ex.Message);

            Console.WriteLine("Error loading UML: " + ex.Message);
        }
    }
}