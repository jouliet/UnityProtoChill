using UnityEditor;
using UnityEngine;

public class MyEditorWindow : EditorWindow
{
    public static event System.Action<string> OnSubmitText;
    private string userInput = ""; 

    [MenuItem("Window/ProtoChillAITool")]
    public static void ShowWindow()
    {
        GetWindow<MyEditorWindow>("ProtoChill Tool");
    }


    private void OnGUI()
    {
        Main.Instance.Init();
        GUILayout.Label("Prompt something so we can generate the object structure you dream of", EditorStyles.boldLabel);

        userInput = GUILayout.TextField(userInput, GUILayout.Height(100)); 

        if (GUILayout.Button("Submit"))
        {
            SubmitText();
        }

        GUILayout.Label("Submitted Text: " + userInput, EditorStyles.label);
    }

    private void SubmitText()
    {
        // Actuellement le seul abonné est l'instance de UMLDiag de main.
        OnSubmitText?.Invoke(userInput); 
    }
}
