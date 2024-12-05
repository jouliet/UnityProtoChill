using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UMLClassDiag;
using System.IO;
using System.Collections.Generic;
using static JsonParser;


public class MyClassTests
{
    [Test]
    public void Test1(){
        string jsonString = File.ReadAllText(@"C:\Users\simon\Documents\PFE\UnityProtoChill\Tests\JsonMockUp.json");
        Debug.Log("Generated UML JSON: " + jsonString);
        //Le cast est nécessaire pour parse
        Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
        //Debug.Log("parsedObject to string : " + ObjectToString(parsedObject));

        //Mapping vers structure objet maison
        BaseObject root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
        //Debug.Log("JSonMapper : " + root.ToString());

        
        GenerateScripts(root);
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
        foreach (var baseObject in baseObjects){
            GenerateScript(baseObject);
        }
    }

    private void GenerateScript(BaseObject _baseObject){
        Debug.Log(_baseObject.ToString());
    }
}


