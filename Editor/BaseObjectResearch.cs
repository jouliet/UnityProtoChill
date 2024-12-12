using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMLClassDiag;

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
}

