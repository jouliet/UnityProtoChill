using UnityEditor;
using UnityEngine;
using static JsonParser;
using System.Collections.Generic;
using ChatGPTWrapper;
using UMLClassDiag;

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
        BaseObject root = null;
        if (gptGenerator == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }
        gptGenerator.GenerateFromText(input, (response) =>
        {
            string jsonString = response;
            Debug.Log("Generated UML JSON: " + jsonString);

            //Le cast est nécessaire pour parse
            Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);

            //Mapping vers structure objet maison
            root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
            //Debug.Log("JSONMApper : " + root.ToString());

            if (root != null){
                //GenerateScripts(root, gptGenerator);
            }
        });
        
        
    }

    //Pour l'instant generateScripts est ici mais on pourra le bouger si besoin. De même pour BaseObjectList
    private void GenerateScripts(BaseObject root, GPTGenerator _gptGenerator){
        List<BaseObject> baseObjects = ObjectResearch.BaseObjectList(root);
        Debug.Log("Generation du premier script");
        //Debug.Log("baseObject.Count = " + baseObjects.Count);
        // foreach (var baseObject in baseObjects){
        //     baseObject.GenerateScript(_gptGenerator);
        // }
        baseObjects[0].GenerateScript(_gptGenerator);
    }

    private static List<BaseObject> BaseObjectList(BaseObject root){
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
}
