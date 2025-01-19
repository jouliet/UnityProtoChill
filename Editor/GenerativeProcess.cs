using UnityEditor;
using UnityEngine;
using ChatGPTWrapper;
using UMLClassDiag;
using ChatClass;

public abstract class GenerativeProcess
{
    // Une classe qui hérite de generative process doit définir OnSubmit qui est le comportement de l'event
    // L'entrée utilisateur est ici un simple string de prompt.

    //Actuellement Idem pour OnGenerateScript qui est géré dans la même editor window et avec la même initialisation de gpt par convéniance

    //l'intance de GPTGenerator est gérée à ce niveau

    public static GPTGenerator gptGenerator;
    protected static string jsonScripts;
    protected static string jsonGOs;
    public static void SetJsonScripts(string json){
        jsonScripts = json;
    }

    public static void SetJsonGOs(string json){
        jsonGOs = json;
    }
    //public abstract void OnSubmit(string input);
    //public abstract void OnGenerateScript(BaseObject root);

    // Generative process est abonné à l'editor window
    public GenerativeProcess()
    {
        if (gptGenerator == null)
        {
            gptGenerator = new GPTGenerator();
        }
        //MyEditorWindow.OnSubmitText += OnSubmit;
        //MyEditorWindow.OnGenerateScriptEvent += OnGenerateScript;
        gptGenerator = new GPTGenerator();
        ChatWindow.OnSubmitText += OnSubmit;
        UIManager.OnGenerateScriptEvent += OnGenerateScript;
    }  
    // Abstract cleanup
    ~GenerativeProcess()
    {
        //MyEditorWindow.OnSubmitText -= OnSubmit;
        //MyEditorWindow.OnGenerateScriptEvent -= OnGenerateScript;
        ChatWindow.OnSubmitText -= OnSubmit;
        UIManager.OnGenerateScriptEvent -= OnGenerateScript;
    }


}


