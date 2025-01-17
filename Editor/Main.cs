#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
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

    public void Init(){
        // La méthode ne fait rien mais permet d'instancier main pour la première fois depuis EditorWindow
        UMLDiag umlDiag = new UMLDiag();
    }

    public void Init(UMLDiagramWindow umlDiagramWindow)
    {
        // La méthode ne fait rien mais permet d'instancier main pour la première fois depuis EditorWindow
        UMLDiag umlDiag = new UMLDiag(umlDiagramWindow);
    }

    // Constructeur privé pour empêcher la création d'instances externes
    private Main()
    {
        // GameObject fastUnityPusher = new GameObject("unityPusher");
        // FastUnityPusher pusherComponent = fastUnityPusher.AddComponent<FastUnityPusher>();
    }

}
#endif