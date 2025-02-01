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

    //NO DOUBLONS I SAID
    public static void Add(BaseObject obj){
    
        var existingObject = AllBaseObjects.FirstOrDefault(o => o.Equals(obj));
        
        if (existingObject == null)
        {
            AllBaseObjects.Add(obj);
        }
        else
        {
            Debug.Log($"objet {obj.Name} doublon grrr");
        }
    }

    public static void AddBGO(BaseGameObject bgo){
        var existingObject = AllBaseGameObjects.FirstOrDefault(o => o.Equals(bgo));
        
        if (existingObject == null)
        {
            AllBaseGameObjects.Add(bgo);
        }
        else
        {
            Debug.Log($"objet {bgo} doublon grrr");
        }
    }
}

