using UnityEditor;
using UnityEngine;
using ChatGPTWrapper;
public class MyEditorWindow : EditorWindow
{
    // Submit prompt to GPT Event
    public static event System.Action<string> OnSubmitText;
    private string userInput = "Make a UML for a platformer in Unity. You shoot bullets at waves of ennemies running at you"; 

    // Init GPT Event
    public static event System.Action<bool, string, string, CustomChatGPTConversation.Model, string> OnInitializeGPTInformation;

    private bool useProxy;
    private string proxyUri = "";
    private string apiKey = "";
    private CustomChatGPTConversation.Model selectedModel = CustomChatGPTConversation.Model.ChatGPT;
    private string initialPrompt = @"You love object abstraction and a big time JSON user. You will follow this exact format : 
{
  ""BaseObject"": {
    ""Name"": ""ObjectName"",
    ""Attributes"": [
      {
        ""Name"": ""AttributeName1"",
        ""Type"": ""AttributeType1"",
        ""DefaultValue"": ""DefaultValue1""
      },
      {
        ""Name"": ""AttributeName2"",
        ""Type"": ""AttributeType2"",
        ""DefaultValue"": ""null""
      }
    ],
    ""Methods"": [
      {
        ""Name"": ""MethodName1"",
        ""ReturnType"": ""ReturnType1"",
        ""Parameters"": [
          {
            ""Name"": ""ParamName1"",
            ""Type"": ""ParamType1"",
            ""DefaultValue"": ""ParamDefaultValue1""
          }
        ]
      },
      {
        ""Name"": ""MethodName2"",
        ""ReturnType"": ""ReturnType2"",
        ""Parameters"": []
      }
    ],
    ""ComposedClasses"": [
      {
        ""Name"": ""ComposedClassName1"",
        ""Attributes"": [
          {
            ""Name"": ""AttributeName1"",
            ""Type"": ""AttributeType1"",
            ""DefaultValue"": ""null""
          }
        ],
        ""Methods"": [],
        ""ComposedClasses"": [],
        ""ParentClass"": ""null""
      }
    ],
    ""ParentClass"": ""null""
  }
}
";


    [MenuItem("Window/ProtoChillAITool")]
    public static void ShowWindow()
    {
        GetWindow<MyEditorWindow>("ProtoChill Tool");
    }

private Vector2 scrollPosition; 
private Vector2 topScrollPosition;
private void OnGUI()
{
    Main.Instance.Init();
    //Title
    GUILayout.Space(10);
    GUILayout.Label("GPT Object Structure Generator", EditorStyles.boldLabel);
    GUILayout.Space(10);

    

    // GPT Initialization Section
    GUILayout.BeginVertical("box");
    GUILayout.Label("Initialize GPT Settings", EditorStyles.boldLabel);
    

    useProxy = EditorGUILayout.Toggle("Use Proxy", useProxy);
    proxyUri = EditorGUILayout.TextField("Proxy URI", proxyUri);
    apiKey = EditorGUILayout.PasswordField("API Key", apiKey);
    selectedModel = (CustomChatGPTConversation.Model)EditorGUILayout.EnumPopup("Model", selectedModel);

    GUILayout.Label("Initial Prompt", EditorStyles.label);
    topScrollPosition = GUILayout.BeginScrollView(topScrollPosition, GUILayout.Height(90));
    initialPrompt = EditorGUILayout.TextArea(initialPrompt, GUILayout.Height(70));
    GUILayout.EndScrollView();
    
    GUILayout.Space(15);

    // Button to Initialize GPT
    if (GUILayout.Button("Initialize GPT", GUILayout.Height(40)))
    {
        InitializeGPTInformation();
        Debug.Log("GPT Initialized with user parameters.");
    }
    GUILayout.EndVertical();

    GUILayout.Space(20);

    // Submit chatbox Section
    GUILayout.BeginVertical("box"); // Begin the box around the text area
    GUILayout.Label("Describe Your Object Structure", EditorStyles.boldLabel);

    // Scrollable text area to allow for scrolling when the content exceeds the space
    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150), GUILayout.ExpandHeight(false)); // Use ExpandHeight for better scroll behavior
    userInput = EditorGUILayout.TextArea(userInput, GUILayout.Height(100));  // Height of the text area where the user will type
    GUILayout.EndScrollView(); // End scroll view
    GUILayout.EndVertical(); // End the vertical box for the text area

    GUILayout.Space(20); // Add spacing between the text area and the button

    // Submit Button Section
    GUILayout.BeginHorizontal(); // Horizontal layout for the button
    if (GUILayout.Button("Submit", GUILayout.Height(40))) // Button with defined height
    {
        SubmitText();
        Debug.Log("Text submitted: " + userInput);
    }
    GUILayout.EndHorizontal(); // End the horizontal layout for the button
}



    private void SubmitText()
    {
        // Actuellement le seul abonné est l'instance de UMLDiag de main.
        OnSubmitText?.Invoke(userInput); 
    }

    private void InitializeGPTInformation()
    {
        // Le seul abonné est l'instance de GPTGenerator, elle même instanciée dans le constructeur de Generative process. On 
        // instancie donc Un GPTGenerator et un CustomChatGPTConversation par generative process. 
        OnInitializeGPTInformation?.Invoke(useProxy, proxyUri, apiKey, selectedModel, initialPrompt);
    }
}
