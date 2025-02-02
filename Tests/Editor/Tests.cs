using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UMLClassDiag;
using System.IO;
using System.Collections.Generic;
using static JsonParser;
using ChatGPTWrapper;
using UnityPusher;
using ChatClass;
using Codice.Client.BaseCommands.Download;

public class DataStructureTests
{
    [Test]
    public void Test1(){
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Tests/JsonMockUp.json");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load JSON.");
            return;
        }
        string jsonString = jsonFile.text;
        Debug.Log("Generated UML JSON: " + jsonString);
        //Le cast est nécessaire pour parse
        Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
        //Debug.Log("parsedObject to string : " + ObjectToString(parsedObject));

        //Mapping vers structure objet maison
        //BaseObject root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
        //Debug.Log("JSonMapper : " + root.ToString());
       
       // GenerateScripts(root);
    }
    
    public static List<BaseObject> BaseObjectList(BaseObject root){
        var baseObjectList = new List<BaseObject>{root};

        if (root.ComposedClasses.Count > 0)
        {
            foreach (var composedClass in root.ComposedClasses)
            {
                baseObjectList.AddRange(BaseObjectList(composedClass));
            }
            
        }
        return baseObjectList;
    }


    private void GenerateScripts(BaseObject root){
        List<BaseObject> baseObjects = ObjectResearch.BaseObjectList(root);
        Debug.Log("Generation des scripts");
        //Debug.Log("baseObject.Count = " + baseObjects.Count);
        // foreach (var baseObject in baseObjects){
        //     GenerateScript(baseObject);
        // }
    }

    private void GenerateScript(BaseObject _baseObject){
        Debug.Log(_baseObject.ToString());
    }
}

// Ici j'ai réimplémenté beaucoup de chose de GPTWrapper car il me fallait initialiser moi même le gptGenerator dans le Test. 

public class gptTest{

    private CustomChatGPTConversation _chatGPTConversation;


    private void GenerateScripts(BaseObject root, GPTGenerator gptGenerator){
        List<BaseObject> baseObjects = ObjectResearch.BaseObjectList(root);
        Debug.Log("Generation d'un script:");
        //Debug.Log("baseObject.Count = " + baseObjects.Count);
        
        GenerateScript(gptGenerator, baseObjects[1]);
    }

    public void GenerateScript(GPTGenerator gptGenerator, BaseObject bo)
    {
        //Peut etre précisé que la classe doit surement hérité de mono behaviour 
        string input = 
        "Tu es dans Unity. Pour le gameObject " + bo.Name + " écris entièrement en c# (avec la fonction Update()) son composant : \n" + bo.ToString() + "\n";

        Debug.Log(bo.Name);

        if (gptGenerator == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }

        GenerateFromText(input, (response) =>
        {
            Debug.Log("Generated class: " + response);
            //WriteScriptFile(response);
            
        });
    }

    public void InitChatGPTConversation(bool useProxy, string proxyUri, string apiKey, CustomChatGPTConversation.Model model, string initialPrompt, float temperature)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.LogError("API key is required to initialize ChatGPTConversation.");
            return;
        }

        _chatGPTConversation = new CustomChatGPTConversation(useProxy, proxyUri, apiKey, model, initialPrompt, 0);

        Debug.Log("ChatGPTConversation initialized successfully.");
    }
    public void GenerateFromText(string text, Action<string> onResponse)
    {
        if (_chatGPTConversation == null)
        {
            Debug.LogError("ChatGPTConversation is not initialized. Call InitChatGPTConversation first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogError("Input text cannot be null or empty.");
            return;
        }

        _chatGPTConversation.SendToChatGPT(text, (response) =>
        {
            Debug.Log($"Message Sent: {text}");
            Debug.Log($"Message Received: {response}");
            onResponse?.Invoke(response);
        });
        

    }

}


public class UMLViewTest
{
    [Test]
    public void BaseObjectUMLGenerationTest()
    {
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Tests/JsonBaseObject.json");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load JSON.");
            return;
        }
        string jsonString = jsonFile.text;
        Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
        // BaseObject root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
        // UMLDiagramWindow.ShowDiagram(root);
    }

    [Test]
    public void JsonMockUpUMLGenerationTest()
    {
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Tests/JsonMockUp.json");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load JSON.");
            return;
        }
        string jsonString = jsonFile.text;
        Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
        //BaseObject root = JsonMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
        //UMLDiagramWindow.ShowDiagram(root);
    }

    [Test]
    public void JsonMockUpUMLGenerationTest2()
    {
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Tests/JsonMockUp2.json");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load JSON.");
            return;
        }
        string jsonString = jsonFile.text;
        Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
        List<BaseObject> baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
        //UMLDiagramWindow.ShowDiagram(baseObjects);
    }
}

public class GameObjectCreatorTest{
    [Test]
    public void gameObjectCreation(){
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Editor/GeneratedContent/currentUML.json");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load JSON.");
            return;
        }
        string jsonString = jsonFile.text;
        // Debug.Log("here is the json file:");
        // Debug.Log(jsonString);
        Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);

        GameObjectCreator.JsonToDictionary(jsonString);
        GameObjectCreator.StockEveryGOsInList();
        GameObjectCreator.CreateAllGameObjects();
        GameObjectCreator.CreateEveryGameObjects(parsedObject);
    }
}

public class NewUMLJsonStructTest{
    [Test]
    public void TestNewMapper(){
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Editor/JsonStructures/Exemples/Classes.json");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load JSON.");
            return;
        }
        string jsonString = jsonFile.text;
        //Debug.Log("here is the json file:");
        //Debug.Log(jsonString);
        Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
        List<BaseObject> bos = JsonMapper.MapAllBaseObjects(parsedObject);
        Debug.Log ("nb classes: " + bos.Count);
        foreach(BaseObject bo in bos){
            Debug.Log(bo);
        }
    }
}

public class LevelDesignTests{
    [Test]
    public void PushThePrefabs(){
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Editor/JsonStructures/LevelDesignStructure.json");
        Dictionary<string, object> LDDict = (Dictionary<string, object>)Parse(jsonFile.text);
        LevelDesignCreator.PushGOsOnScene(LDDict);
    }
}




