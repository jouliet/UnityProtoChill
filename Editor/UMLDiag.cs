using UnityEditor;
using UnityEngine;

public class UMLDiag : GenerativeProcess
{
    public UMLDiag()
    {
        // UML est abonné si tout se passe bien
        Debug.Log("UMLDiag process initialized.");
    }

    public override void OnSubmit(string input)
    {
        Debug.Log("Submit received in UMLDiag. Generating UML...");
        GenerateUML(input);
    }

    private void GenerateUML(string input)
    {
        string jsonResult = GPTGenerator.GenerateUML(input);
        Debug.Log("Generated UML JSON: " + jsonResult);
    }
}
