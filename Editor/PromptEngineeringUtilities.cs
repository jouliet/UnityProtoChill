using UnityEngine;
using System.IO;
using static SaverLoader;
using UMLClassDiag;
public static class PromptEngineeringUtilities
{

    //Initial prompt
    public static string staticOverridenInitialPrompt = "You are a game developer specialized in Unity. You will be asked to write jsons which you will put in json markers (```json ... ```). Always make a summary of your answers and put it in 'user' markers (```user ... ```). Only ever include at most 1 json and 1 user summary per answer. If you are tasked to write a script, put it in ```csharp ... ``` markers. ";

    //Structure de donnée pour la génération initiale
    private static string classesAndGOJsonStructurePath = "Packages/com.jin.protochill/Editor/JsonStructures/Classes&GOStructure.json";

    public static string UMLAndGOPrompt(string inputUser) 
    {


     
        string output;
        string currentUml = "nada";
        try {currentUml = File.ReadAllText(UMLFilePath); }
        catch{Debug.Log("No UML Yet, Generating for the first time");}
        string classesAndGOJsonStructure;
        string separationRequest = "The UML in the 'Classes' node describe the scripts and the 'GameObjects' node describes the prefabs and it's components, which are scripts defined in Classes or Unity specific components. Their properties are also described, you may modify values inside the properties ";
        string classesRequest = "For the Classes part: \n" +
        "Composed Classes are the classes used by the class\n" +
        "Classes with the most composed classes should be at the top of the list.";
        string goRequests = 
        "For the GameObject part : \n" +
        "Float values format exemple : 10.5 \n" +
        "Don't hesitate to add boxCollider, rigidbody or any Unity specific component. Also don't hesisate to use the properties : scale the gameobject with the Transform localScale, or edit the values of a Script component.\n" +
        "You must add MeshFilter (with MeshRenderer) component on almost all game objects who are not UI or Managers.";
        string inputToCreatePrefabs = 
        "Remember that the script names must be coherent with the UML scripts. \n";

        if (currentUml == "nada"){ // Cas de première génération
            classesAndGOJsonStructure = 
            "You are a Json Writer. You will follow this exact format with every value in between quotes : \n" +
            File.ReadAllText(classesAndGOJsonStructurePath);
            output = classesAndGOJsonStructure+separationRequest+classesRequest+goRequests+inputToCreatePrefabs + inputUser;
            
        }
        else { //Cas d'update
            classesAndGOJsonStructure = currentUml;
            output = currentUml + "Modify this Json File to achieve this goal : " + inputUser + ". Before answering with the json, answer this question : Do you need to modify the Classes or their properties in the GameObjects Node ? You may comment your operations on the Json to reaffirm this goal";
        }
        
        
        
        return (output);
    }

    //Prompt spécifique pour générer un script
    public static string ScriptGenerationPrompt(string name, string jsonOfSelfAndNeighbors)
    {
        return "You are in Unity, write this c# class :" + name + "as described in this json : " + jsonOfSelfAndNeighbors + @"You only use functions defined in the uml or native to Unity (Start and Update should be used to initialize and update objects over time).
        Never assume a method, class or function exists unless specified in the uml. Relevant gameObjects and prefabs will always be named ObjectNameGO. Use ```csharp marker. ";
    }

    public static string UpdateSingleClassPrompt(BaseObject bo, string inputuser){
        return ("Modify the object : " + "{ Classes: [ " + bo.ToJson() + " ]}"  +  "\n"+ "Follow these instructions : " + inputuser + "\n");
    }
}