#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

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
    }

    // Constructeur privé pour empêcher la création d'instances externes
    private Main()
    {
        UMLDiag umlDiag = new UMLDiag();
        // GameObject fastUnityPusher = new GameObject("unityPusher");
        // FastUnityPusher pusherComponent = fastUnityPusher.AddComponent<FastUnityPusher>();
    }

}
#endif