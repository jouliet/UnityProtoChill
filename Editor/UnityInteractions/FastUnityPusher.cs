using System.Collections;
using System.Collections.Generic;
using UMLClassDiag;
using UnityEngine;
using UnityEditor;

public class FastUnityPusher 
{
    public void CreateGameObjectAtZerosFromBaseObject(BaseObject baseObject){
        string ScriptName = baseObject.Name;

        
        GameObject go = new GameObject(ScriptName+"GO");
        BoxCollider boxCollider = go.AddComponent<BoxCollider>();
        boxCollider.center = Vector3.zero;

        //Mesh
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.SetParent(go.transform);  
        capsule.transform.localPosition = Vector3.zero;  
        
        System.Type MyScriptType = System.Type.GetType (ScriptName + ",Assembly-CSharp");
        go.AddComponent(MyScriptType);
        PrefabUtility.InstantiatePrefab(go);
    }
}
