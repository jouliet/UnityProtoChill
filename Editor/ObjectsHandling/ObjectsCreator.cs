using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using static JsonParser;
using static UIManager;
using static SaverLoader;
using static ObjectResearch;
using System;
using static ScriptsCoherenceHandler;
using UMLClassDiag;
using static UMLDiag;
public static class ObjectsCreator
{
    // Tout ça était dans le bouton magique mais méritait vraiment d'être rangé c'est la vue la haut niveau de la création d'objets et devrait être 
    // Regardée en priorité
    public static void CreateAllObjectsAndReload() {

        if (!File.Exists(UMLFilePath)){ 
        //This will Make all objects (baseobjects and base GameObjects that have an equivalent in the Unity Project)
        ScriptsCoherenceHandler.UpdateBaseObjectsToMatchProject();
        PrefabCoherenceHandler.UpdateBaseGameObjectsToMatchUnityProject();
        UMLDiag.SaveDataToCurrentUML();
        return;
        }
        var jsonFile = File.ReadAllText(UMLFilePath);

        //Cast pour parse
        string jsonString = jsonFile;
        Dictionary<string, object> parsedObject  = (Dictionary<string, object>)Parse(jsonString);
        // This will make all objects and gameobjects : those in the json file and those in the unity project
        List<BaseObject> baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
        JsonMapper.MapAllBaseGOAndLinksToBO(parsedObject);

        //Those objects shouldnt be the same most of the time, but if they are, the coherence handler are called after so
        // BaseObjectResearch decides what to keep. As of writing, it keeps the newest received and replaces the old one.
        ScriptsCoherenceHandler.UpdateBaseObjectsToMatchProject();
        PrefabCoherenceHandler.UpdateBaseGameObjectsToMatchUnityProject();

        // Security for UI Reasons (basically we rly want that green button to work even if mapping fails)
        foreach(BaseObject bo in AllBaseObjects){
                bo.refreshStateToMatchUnityProjet();
            }

        //On garde l'UML au cas ou on neuil (jcrois on neuil ou on va bientôt neuil jvais craquer jvais saver data ici et écraser json)

        GenerativeProcess.SetJsonScripts(jsonString);

        
    }
}
