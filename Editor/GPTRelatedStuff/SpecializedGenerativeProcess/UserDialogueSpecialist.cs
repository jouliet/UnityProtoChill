using UnityEngine;

public class DialogueSpecialist : GenerativeProcess
{
    public void GenerateCoolandYetUsefulUserDialogue(string projectContext, string conversationHistory)
    {
        gPTGenerator.GenerateFromText(projectContext +"\n"+ conversationHistory, (response) =>
        {
            Debug.Log("Response: " + response);
        });
    }
}