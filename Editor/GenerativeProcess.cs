using UnityEditor;
using UnityEngine;
using ChatGPTWrapper;
public abstract class GenerativeProcess
{
    // Une classe qui hérite de generative process doit définir OnSubmit qui est le comportement de l'event
    // L'entrée utilisateur est ici un simple string de prompt.

    //l'intance de GPTGenerator est gérée à ce niveau

    public GPTGenerator gptGenerator;
    public abstract void OnSubmit(string input);
    
    // Generative process est abonné à l'editor window
    public GenerativeProcess()
    {
        gptGenerator = new GPTGenerator();
        MyEditorWindow.OnSubmitText += OnSubmit;
    }  
    // Abstract cleanup
    ~GenerativeProcess()
    {
        MyEditorWindow.OnSubmitText -= OnSubmit;
    }


}


