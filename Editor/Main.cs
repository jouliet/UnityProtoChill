#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityPusher;
using UMLClassDiag;

public class Main
{
    // Pattern singleton
    private static Main _instance;

    public static Main Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Main();
            }
            return _instance;
        }
    }


    public void Init(UMLDiagramWindow umlDiagramWindow)
    {
        ObjectResearch.CleanUp();
        UMLDiag.Initialize(umlDiagramWindow);
    }

    // Constructeur privé pour empêcher la création d'instances externes
    private Main()
    {
        GameObjectCreator gameObjectCreator = new GameObjectCreator();

        // GameObject fastUnityPusher = new GameObject("unityPusher");
        // FastUnityPusher pusherComponent = fastUnityPusher.AddComponent<FastUnityPusher>();
    }

}
#endif