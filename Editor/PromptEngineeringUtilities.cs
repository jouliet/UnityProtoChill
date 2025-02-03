using UnityEngine;
using System.IO;
using static SaverLoader;
using UMLClassDiag;
public static class PromptEngineeringUtilities
{
    static bool FirstGen = true;
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
        "Classes with the most composed classes should be at the top of the list.\n" +
        "Attributes may and should reference gameObjects in the GameObjects Node.\n" +
        "When you create a class you will most likely need to add it to existing prefab gameObjects in the GameObjects node that should have this behavior, or create one if there's none yet";
        string goRequests = 
        @"

        For the GameObject part:
        Float values format example: 10.5
        Don't hesitate to add boxCollider, rigidbody, or any Unity-specific component. Also, don't hesitate to use the properties: scale the GameObject with the Transform localScale, or edit the values of a Script component.
        You must add MeshFilter (with MeshRenderer) component on almost all game objects that are not UI or Managers. Only use Primitive Meshes.
        ";
        
        string inputToCreatePrefabs = 
        "Remember that the script names must be coherent with the UML scripts. \n";

        if (FirstGen){ // Cas de première génération
            classesAndGOJsonStructure = 
            "You are a Json Writer. You will follow this exact format with every value in between quotes : \n" +
            File.ReadAllText(classesAndGOJsonStructurePath);
            output = classesAndGOJsonStructure+separationRequest+classesRequest+goRequests+inputToCreatePrefabs + inputUser;
            FirstGen = false;
        }
        else { //Cas d'update
            classesAndGOJsonStructure = currentUml;
            output = currentUml + goRequests + "Modify this Json File to achieve this goal : " + inputUser + ". Begin by answering this question : Do you need to modify the Classes or their properties in the GameObjects Node ? You may comment your operations on the Json to reaffirm this goal. Now on to Json Generation : ";
        }
        
        
        return (output);
    }

    //Prompt spécifique pour générer un script
    public static string ScriptGenerationPrompt(string name, string jsonOfSelfAndNeighbors, string userInput)
    {
        return "You are in Unity, write this c# class :" + name + "as described in this json : " + jsonOfSelfAndNeighbors + @"You only use functions defined in the uml or native to Unity (Start and Update should be used to initialize and update objects over time).
        Never assume a method, class or function exists unless specified in the uml. Use GameObjects rather than Transforms as fields and then access the transforms with go.transform. Keep your fields public. Relevant gameObjects and prefabs will always be named ObjectNameGO. Use ```csharp marker. " + userInput;
    }

    public static string UpdateSingleClassPrompt(BaseObject bo, string inputuser){
        return ("Modify the object : " + "{ Classes: [ " + bo.ToJson() + " ]}"  +  "\n"+ "Follow these instructions : " + inputuser + "\n" + "Adding a new class means writing a similar structure for it in the Classes list, and being careful to use the same name in ComposedClasses");
    }

    public static string ComposedClassesString(){ return "Here are the attribut and methods of the composed class you may use to interact with others objects : "; }
}