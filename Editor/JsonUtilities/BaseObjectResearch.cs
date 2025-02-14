using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UMLClassDiag;
using System.Linq;
public class ObjectResearch
{
    private static ObjectResearch _instance;

    // Propriété pour accéder à l'instance unique
    public static ObjectResearch Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ObjectResearch();
            }
            return _instance;
        }
    }

    public static List<BaseObject> AllBaseObjects = new List<BaseObject>();
    public static List<BaseGameObject> AllBaseGameObjects = new List<BaseGameObject>();
    public static BaseObject FindBaseObjectByName(string name){
        foreach (var baseObject in AllBaseObjects) {
            if (baseObject.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                return baseObject;
            }
    
    }
    return null;

    }
    public static BaseGameObject FindBaseGameObjectByName(string name){
        foreach (var baseGameObject in AllBaseGameObjects) {
            if (baseGameObject.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                return baseGameObject;
            }
    
    }
    return null;

    }
    public static BaseObject BaseObjectResearch(BaseObject root, string className)
    {
        string baseObjectName = "";
        BaseObject baseObject = null;
        if (baseObject.Name != baseObjectName){
            if (baseObject.ComposedClasses.Count > 0){
                foreach (var composedClass in baseObject.ComposedClasses){
                    baseObject = BaseObjectResearch(composedClass, className);
                    if (baseObject != null){
                        return baseObject;
                    }
                }
            }else{
                baseObject = null;
            }
        }else{
            return baseObject;
        }
        return baseObject;
    }

    public static void CleanUp(){
        AllBaseObjects = new List<BaseObject>();
        AllBaseGameObjects = new List<BaseGameObject>();
    }

    //NO DOUBLONS I SAID 
    public static void Add(BaseObject obj){
        
        var existingObjectIndex = AllBaseObjects.FindIndex(o => o.Equals(obj));

        if (existingObjectIndex == -1) //Pas trouvé
        {
            AllBaseObjects.Add(obj);
        }
        else
        {
            AllBaseObjects[existingObjectIndex] = obj; //replace
        }
    }

    public static void AddBGO(BaseGameObject bgo){
        var existingObjectIndex = AllBaseGameObjects.FindIndex(o => o.Equals(bgo));
        
        if (existingObjectIndex == -1)
        {
            AllBaseGameObjects.Add(bgo);
        }
        else
        {
            AllBaseGameObjects[existingObjectIndex] = bgo;
        }
    }

    public static string AllBaseObjectsToCleanString(){
        string allBo = "";
        foreach(BaseObject bo in AllBaseObjects){
            allBo+= bo.ToString();
            allBo+="\n";
        }

        return allBo;

    }

    public static string AllBaseGameObjectsToCleanString(){
        string AllBgo = "";
        foreach(BaseGameObject bgo in AllBaseGameObjects){
            AllBgo+= bgo.ToString();
            AllBgo+="\n";
        }
        return AllBgo;
    }
}

