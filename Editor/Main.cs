using UnityEditor;
using UnityEngine;

public class Main
{
    private UMLDiag umlDiag; 

    // On instancie les process et on les inscris aux events dans le constructeur de main.
    // Main est instancié depuis EditorWindow.
    // Que faire à la destruction ?! 
    public Main()
    {
        umlDiag = new UMLDiag();
        MyEditorWindow.OnSubmitText += umlDiag.OnSubmit;
        Debug.Log("Main: UMLDiag has been instantiated and event subscribed.");
    }

    public void Unsubscribe()
    {
        MyEditorWindow.OnSubmitText -= umlDiag.OnSubmit;
        Debug.Log("Main: UMLDiag unsubscribed from events.");
    }
}
