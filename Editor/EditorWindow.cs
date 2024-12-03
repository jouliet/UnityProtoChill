using UnityEditor;
using UnityEngine;

public class MyEditorWindow : EditorWindow
{
    // Principale instance de main instanciée à l'ouverture de la fenêtre. La fermeture de la fenêtre 
    // Actuellement unsubscribe main mais ne détruit pas les objets. 
    private Main mainController;

    // OnSubmitText is the prompt input
    public static event System.Action<string> OnSubmitText;
    private string userInput = ""; 

    [MenuItem("Window/ProtoChillAITool")]
    public static void ShowWindow()
    {
        GetWindow<MyEditorWindow>("ProtoChill Tool");
    }

    private void OnEnable()
    {
        mainController = new Main();
        Debug.Log("MyEditorWindow: Main instance created.");
    }

    private void OnDisable()
    {
        if (mainController != null)
        {
            mainController.Unsubscribe();
            Debug.Log("MyEditorWindow: Main instance unsubscribed.");
        }
    }


    private void OnGUI()
    {
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
        Debug.Log("User input submitted: " + userInput);
    }
}
