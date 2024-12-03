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
        MyEditorWindow.OnSubmitText += umlDiag.OnSubmit;
    }

}
