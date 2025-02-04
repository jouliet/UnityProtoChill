using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ChatGPTWrapper;
using System.Text.RegularExpressions;
using static SaverLoader;
using static ObjectResearch;
using System.Linq;
using System;
using Unity.VisualScripting.YamlDotNet.Core.Events;
using static JsonParser;
using UnityPusher;
using Unity.VisualScripting;
using static PromptEngineeringUtilities;

namespace UMLClassDiag
{


public class BaseGameObject
{
    public string Name;
    public string Tag;
    public string Layer;
    public List<BaseObject> Components = new List<BaseObject>(); 

    public BaseGameObject(string Name, string Tag= "Untagged", string Layer="Default") {
        this.Name = Name;
        this.Tag = Tag;
        this.Layer = Layer;
        ObjectResearch.AddBGO(this);
    }
    public string ToJson()
    {
        if (Name == "Unknown"){
            Debug.LogWarning("An unidentified gameObject was generated, consider verifying your prefabs and retrying");
        }
        var componentsJson = string.Join(",\n\t\t", Components.Select(c => c.ToJsonGOSpecific()));

        return $"{{\n" +
            $"\t\"name\": \"{Name}\",\n" +
            $"\t\"tag\": \"{Tag}\",\n" +
            $"\t\"layer\": \"{Layer}\",\n" +
            $"\t\"components\": [\n\t\t{componentsJson}\n\t]\n" +
            $"}}";
    }

    public override string ToString()
    {
        string componentsstring = "\n Components :";
        if (Components.Count>0){
            foreach(BaseObject component in this.Components){
                componentsstring += component.Name;
            }
        }else {
            componentsstring = "";
        }

        string Tagstring ="";
        if (Tag!= "Untagged"){
            Tagstring = "Tag : " + Tag; 
        }
        string Layerstring = "";
        if (Layer!="Default"){
            Layerstring = "Layer : "+Layer;
        }

        return "name : "+Name+", Tag :"+Tag+"Layer, "+Layer  + componentsstring;
    }

    public override bool Equals(object obj)
    {
        if (obj is BaseGameObject other)
        {
            return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }
    public void GeneratePrefab(){
        if (this.Name == null){
            Debug.Log("Problème");
        }
        GameObject go = new GameObject(this.Name)
        {
            tag = this.Tag,
            layer = LayerMask.NameToLayer(this.Layer)
        }; 


        foreach(BaseObject comp in Components){
            comp.DirectAddScriptToGameObject(go);
        }
        

        if (!Directory.Exists( GameObjectCreator.prefabPath))
        {
            Directory.CreateDirectory( GameObjectCreator.prefabPath);
            Debug.Log("Dossier créé : " +  GameObjectCreator.prefabPath);
        }
        // Sauvegarder le GameObject en tant que prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, GameObjectCreator.prefabPath + "/" + this.Name + ".prefab");
        if (prefab ==null){
            Debug.Log("problème");
        }

        GameObject.DestroyImmediate(go);
    }


}



public class Attribute
{
    public string Name { get; set; } 
    public string Type { get; set; } 

    public override string ToString()
    {
        if (Type == "GameObject" || Type =="gameObject") {

            BaseGameObject bgo = ObjectResearch.FindBaseGameObjectByName(Name);
            if (bgo == null) {
                //On tente sait on jamais. On le fait pas dans le json pcq vsy
                //Faudrait standardiser ça quoi ff at this point todo
                bgo = new BaseGameObject(Name);
            }
            return "Attribute"+ bgo.ToString();
        }
        return $"Attribute Name: {Name}, Type: {Type}";
    }

    public string ToJson()
    {
        return $"{{\n" +
            $"\t\"Name\": \"{Name}\",\n" +
            $"\t\"Type\": \"{Type}\"\n" +
            $"}}";
    }
}

public class Method
{
    public string Name { get; set; } 
    public string ReturnType { get; set; } 
    public List<Attribute> Parameters { get; set; } = new List<Attribute>(); 

    public override string ToString()
    {
        string parametersStr = Parameters.Count > 0 ? string.Join(", ", Parameters) : "None";
        return $"Method Name: {Name}, Return Type: {ReturnType ?? "None"}, Parameters: [{parametersStr}]";
    }

    public string ToJson()
    {
        var parametersJson = string.Join(",\n\t\t", Parameters.Select(p => p.ToJson()));

        return $"{{\n" +
            $"\t\"Name\": \"{Name}\",\n" +
            $"\t\"ReturnType\": \"{ReturnType}\",\n" +
            $"\t\"Parameters\": [\n\t\t{parametersJson}\n\t]\n" +
            $"}}";
    }
}
  

public class BaseObject : GenerativeProcess
{
    // On constitue la liste localement
    public BaseObject(string Name, bool isReal = false){
        this.Name = Name;
        this.isReal = isReal;
        if (!isReal){
            ObjectResearch.Add(this);
        } 
        scriptType = Type.GetType($"{Name}, Assembly-CSharp");

        if (scriptType != null) {
            hasBeenGenerated = true;
        }
        else {
            scriptType = Type.GetType($"UnityEngine.{Name}, UnityEngine");
            if (scriptType != null){
                isSpecificUnityComponent = true;
                hasBeenGenerated = true;
                // Debug.Log("Component : "+ Name + " is now recognized");
            }
        }
    }


    public void refreshStateToMatchUnityProjet(){
        Type scriptType = Type.GetType($"{this.Name}, Assembly-CSharp");

        if (scriptType != null) {
            hasBeenGenerated = true;
        }
    }

    public string ToJson()
    {
        if (this.isReal){
            Debug.Log("très sus");
        }
        var attributesJson = Attributes.Any() 
        ? string.Join(",\n\t\t", Attributes.Select(a => a.ToJson())) 
        : string.Empty;

        var methodsJson = Methods.Any() 
            ? string.Join(",\n\t\t", Methods.Select(m => m.ToJson())) 
            : string.Empty;

        var composedClassesJson = ComposedClasses.Any() 
            ? string.Join(",\n\t\t", ComposedClasses.Select(c => $"\"{c.Name}\"")) 
            : string.Empty;
 
        return $"{{\n" +
            $"\t\"Name\": \"{Name}\",\n" +
            $"\t\"Attributes\": [{attributesJson}],\n" +
            $"\t\"Methods\": [{methodsJson}],\n" +
            $"\t\"ComposedClasses\": [{composedClassesJson}],\n" +
            $"\t\"ParentClass\": \"{ParentClass}\"\n" +
            $"}}";

    }

    public string ToJsonGOSpecific()
    {
        var propertiesJson = string.Join(",\n\t\t", Properties.Select(kv => 
            kv.Value is System.Collections.IEnumerable && kv.Value is not string
                ? $"\"{kv.Key}\": [\n\t\t\t{string.Join(",\n\t\t\t", ((IEnumerable<object>)kv.Value).Select(v => $"\"{v}\""))}\n\t\t]"
                : $"\"{kv.Key}\": \"{kv.Value}\""
        ));

        return $"{{\n" +
            $"\t\"type\": \"{Name}\",\n" +
            $"\t\"properties\": {{\n\t\t{propertiesJson}\n\t}}\n" +
            $"}}";
    }

    ~BaseObject(){
        AllBaseObjects.Remove(this);
    }
    
    public override bool Equals(object obj)
    {
        if (obj is BaseObject other)
        {
            return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    // Implémenter GetHashCode pour garantir que l'égalité fonctionne dans les collections
    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }

    public string Name { get; set; }
    public List<Attribute> Attributes { get; set; } = new List<Attribute>();
    public List<Method> Methods { get; set; } = new List<Method>();
    public List<BaseObject> ComposedClasses { get; set; } = new List<BaseObject>();
    public BaseObject ParentClass { get; set; }
    public List<string> GameObjectAttachedTo { get; set; } = new List<string>();
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    private bool isReal;
    public bool hasBeenGenerated = false;
    public bool isSpecificUnityComponent;
    public string scriptFile;
    public Type scriptType;
    public void InitWithProperties(Dictionary<string, object> propertiesDict)
    {
           
        foreach (var property in propertiesDict)
        {
            if (this.Properties.ContainsKey(property.Key))
            {
                this.Properties[property.Key] = property.Value; 
            }
            else
            {
                this.Properties.Add(property.Key, property.Value);
            }
        }
   
    if (!this.isSpecificUnityComponent)  // Si ce n'est pas un composant Unity, mettre à jour les attributs aussi
    {

        foreach (var property in propertiesDict)
        {
            var attribute = this.Attributes.FirstOrDefault(a => a.Name == property.Key);
            if (attribute != null)
            {

                
            }
            else
            {
                this.Attributes.Add(new Attribute
                {
                    Name = property.Key,
                    Type = property.Value.GetType().Name
                });
            }
        }
    }
}


    // Appelé par UMLDiag ligne 110
    public void GenerateScript(string input){
        GenerateScriptbis(input);
        // Des trucs s'exécutent en parallèle voir le moment ou "oui" arrive (bien plus tôt que les debug de generate bis)

        //Debug.Log("oui");
    }
      
    // Bordel à refact dans une classe composée scriptGenerator ou dans unityPusher. Gaffe alors au timing de l'exécution de RefreshDatabase
    private void GenerateScriptbis(string inputUser)
    {
        string composedClassesString = ComposedClassesString();
        if (ComposedClasses.Count>0){
            foreach(BaseObject composedClass in this.ComposedClasses){
                composedClassesString += composedClass.ToString();
                composedClassesString += "\n";
            }
        }else {
            composedClassesString = "";
        }
        
        //Peut etre précisé que la classe doit au moins indirectement hériter de mono behaviour 
        string input = ScriptGenerationPrompt(this.Name, this.ToString() + composedClassesString, inputUser, this.scriptFile);
        
        if (gPTGenerator == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }
        gPTGenerator.GenerateFromText(input, (response) =>
        {
            Debug.Log("Generated class: " + response);
            WriteScriptFile(GetScript(response));
        });
    }
    private void WriteScriptFile(string content)
    {
        // Chemin du dossier et du fichier
        string filename = this.Name + ".cs";
        string folderPath = "Assets/Scripts";
        string filePath = Path.Combine(folderPath, filename);
        // Vérifie si le dossier existe, sinon le crée
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Dossier créé : " + folderPath);
        }

        File.WriteAllText(filePath, ExtractCSharpCode(content));
        Debug.Log("Fichier créé : " + filePath);
        
        // REFRESH DATABASE ICI
        // Important que ce soit à la fin comme ça !!!! (pour des raisons obscures peut être multithread unity bizarre)
         #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        this.hasBeenGenerated = true;
        //LoadUML();
        #endif
    }

    public void DirectAddScriptToGameObject(GameObject newPrefab){
        
        if (scriptType == null)
            {
                scriptType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => t.Name == Name);
                    //Debug.LogWarning("Not finding component : "+ Name + " . Did you forget to generate it ? Interact with the graph !");
            }
        else{
            Component newComponent;
            if (scriptType != typeof(Transform)){
                    newComponent = newPrefab.AddComponent(scriptType);
                }else{
                    newComponent = newPrefab.transform;

                }
                
            if (this.isSpecificUnityComponent){
                GameObjectCreator.AddPropertiesToComponent(Properties, newComponent, scriptType);
            }
            else{
                try {
                
                    GameObjectCreator.AddFieldsToComponent(Properties, newComponent, scriptType);
                }
                catch (Exception e) {Debug.LogWarning("Adding" + scriptType.Name + "to" + newComponent.name +"Exception :" +e.Message);}
            }
        } 
    }

    public void AddScriptToGameObject(string prefabPath)
    {
        // Charger le prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            //Debug.LogError("Prefab introuvable : " + prefabPath);

            return;
        }

        // Obtenir le type du script
        Type scriptType = Type.GetType($"{this.Name}, Assembly-CSharp");
        if (scriptType == null)
        {
            //Debug.LogError("Script introuvable : " + this.Name);
            return;
        }

        // Instancier temporairement le prefab pour modifier ses composants
        //GameObject tempInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (prefab.GetComponent(scriptType) == null) // Éviter les doublons
        {
            prefab.AddComponent(scriptType);
        }

        // Appliquer les modifications au prefab
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        //Debug.Log(tempInstance == null);

        // if (tempInstance != null){
        //     //Debug.Log("attempting to destroy go : " + tempInstance.name);
        //     //GameObject.DestroyImmediate(tempInstance); // Nettoyage
            
        //     Debug.Log($"Script {this.Name} ajouté au prefab {prefabPath}.");
        // }
        // else {
        //     Debug.LogWarning("une Instance de prefab traine peut être dans la scène");
        //     EditorApplication.QueuePlayerLoopUpdate();
        //     SceneView.RepaintAll();
        // }
    }
    
    private string ExtractCSharpCode(string input)
    {
        // Expression régulière pour capturer le contenu entre ```csharp et ```
        var match = Regex.Match(input, @"```csharp\s*(.*?)\s*```", RegexOptions.Singleline);

        // Retourne le contenu correspondant ou un message vide
        return match.Success ? match.Groups[1].Value : "//Aucun script C# trouvé.";
    }

    public string RelanvantInfo(){
        string result = "";
        return result;
    }
    
    public override string ToString()
    {
        //string result = $"BaseObject : {Name}\n";
        string result = $"Name  - {Name}\n";
        result += "Attributes:\n";
        foreach (var attribute in Attributes)
        {
            result += $"  - {attribute}\n";
        }
        result += "Methods:\n";
        foreach (var method in Methods)
        {
            result += $"  - {method}\n";
        }
        if (ComposedClasses.Count > 0)
        {
            result += "Composed Classes:\n";
            foreach (var composedClass in ComposedClasses)
            {
                result += $"  - {composedClass.Name}\n";
            }
        }
        if (ParentClass != null)
        {
            result += $"Parent Class: {ParentClass.Name}\n";
        }
        return result;
    }


}

public class JsonMapper{

    // List of Dictionary object of classes
    public static List<object> Classes;

    // List of baseobjects classes



    public static BaseObject TrackBaseObjectByName(string className){
        foreach (BaseObject bo in AllBaseObjects){
            if (bo.Name == className){
                return bo;
            }
        }
        return null;
    }
    public static BaseObject MapPreciseBaseObject(List<object> jsonList, string className){
        var baseObject = TrackBaseObjectByName(className);
        if (baseObject != null){
            return baseObject;
        }
        if (className != "null")
        {
            Debug.LogWarning("The object class " + className  + " doesn't exist. Generating it now and updating incomplete json...");
            BaseObject bo = new BaseObject (className);
            //bo.ToJson();
            return bo;
        }
        
        return null;
    }

public static List<BaseGameObject> MapAllBaseGOAndLinksToBO(Dictionary<string, object> jsonDict)
    {
        List<BaseGameObject> mappedGameObjects = new List<BaseGameObject>();
        
        if (jsonDict.ContainsKey("GameObjects") && jsonDict["GameObjects"] is List<object> gameObjects)
        {
            
            foreach (object obj in gameObjects)
            {
                if (obj is Dictionary<string, object> gameObjectDict)
                {
                    
                    string name = gameObjectDict.ContainsKey("name") ? gameObjectDict["name"].ToString() : "Unknown";
                    string Tag = gameObjectDict.ContainsKey("tag") ? gameObjectDict["tag"].ToString() : "Untagged";
                    string Layer = gameObjectDict.ContainsKey("layer") ? gameObjectDict["layer"].ToString() : "Default";


                    UsefulFunctions.EnsureTagExists(Tag);
                    UsefulFunctions.EnsureLayerExists(Layer);
    

                    BaseGameObject baseGameObject = new BaseGameObject(name, Tag, Layer);
                    
                    if (gameObjectDict.ContainsKey("components") && gameObjectDict["components"] is List<object> componentList)
                    {
                        foreach (object component in componentList)
                        {
                            if (component is Dictionary<string, object> componentDict)
                            {
                                if (componentDict.ContainsKey("type") && componentDict["type"] is string type)
                                {
                                    BaseObject baseObject = new BaseObject(type, true);
                                    
                                    if (baseObject != null){

                                        if (componentDict.ContainsKey("properties") && componentDict["properties"] is Dictionary<string, object> propertiesDict)
                                        {
                                            baseObject.InitWithProperties(propertiesDict);
                                        }
                                        
                                        baseGameObject.Components.Add(baseObject);
                                    }
                                    else {
                                        //Cas nul (pas de baseobject connu, donc un composant spécifique unity, c'est ici qu'on va générer le baseObject du coup):
                                        if (type != "Transform"){
                                            Debug.Log("Mapping Unity Component  : " +type );
                                            BaseObject bo = new BaseObject(type, true);
                                            if (componentDict.ContainsKey("properties") && componentDict["properties"] is Dictionary<string, object> propertiesDict)
                                            {
                                                bo.Properties = propertiesDict;
                                                bo.hasBeenGenerated = true;
                                                bo.isSpecificUnityComponent = true;
                                            }

                                            baseGameObject.Components.Add(bo);

                                        }
                                    }
                                }
                            }
                        }
                    }
                    

                    mappedGameObjects.Add(baseGameObject);
                    baseGameObject.GeneratePrefab();
                }
            }
        }
        return mappedGameObjects;
    }


    public static List<BaseObject> MapAllBaseObjects(Dictionary<string, object> jsonDict){
        List<BaseObject> BaseObjects = new List<BaseObject>();
        
        BaseObject bo;
        if (jsonDict == null){
            throw new Exception("Dictionary of classes no defined: Unable to create baseObjects");
        }

        if (jsonDict.ContainsKey("Classes") && jsonDict["Classes"] is List<object> classes)
        {
            if (Classes == null){
                Classes = (List<object>)jsonDict["Classes"];
            }
            foreach (var _class in classes) 
            {
                if (_class is Dictionary<string, object> classDict)
                {
                    bo = MapToBaseObject(classDict, jsonDict);

                    foreach (string goName in bo.GameObjectAttachedTo)
                    {
                        // Debug.Log("Le script " + bo.Name + "s'ajoute au gameobject " + goName);
                        //bo.AddScriptToGameObject(GameObjectCreator.prefabPath + "/" + goName + ".prefab");
                    }
                }
            }
            AddComposedClasses(classes);
            return BaseObjects;
        }else{
            Debug.LogError("Le json est mal écrit à la racine. Le parser ne fonctionne pas!");
            return null;
        }
    }

    public static void AddComposedClasses(List<object> classes){
        foreach (var _class in classes) 
        {
            if (_class is Dictionary<string, object> _classDict)
            {
                string className = _classDict.ContainsKey("Name") ? _classDict["Name"].ToString() : string.Empty;
                BaseObject bo = TrackBaseObjectByName(className);
                if (bo == null){
                    throw new Exception("La classe " + className + " n'existe pas." );
                }
                bo.ComposedClasses = MapComposedClasses(_classDict);
            }
        }
    }

    public static BaseObject MapToBaseObject(Dictionary<string, object> classDict, Dictionary<string,object> jsonDict)
    {
        string Name = classDict.ContainsKey("Name") ? classDict["Name"].ToString() : string.Empty;
        var baseObject = new BaseObject(Name)
        {
            Attributes = MapAttributes(classDict),
            Methods = MapMethods(classDict),
            // Les classes composées seront ajoutées après. Si on le fait tout de suite, 
            // ca fait une boucle infini.
        };
        baseObject.GameObjectAttachedTo = MapGameObjectAttachedTo(jsonDict, baseObject.Name);
    
        return baseObject;
    }

    public static List<string> MapGameObjectAttachedTo(Dictionary<string, object> jsonDict, string boname){
        List<string> list = new List<string>();
        if (jsonDict.ContainsKey("GameObjects") && jsonDict["GameObjects"] is List<object> classes)
        {
            foreach(object obj in classes)
            {
                if (obj is Dictionary<string,object> gameObjectDict){
                    if (gameObjectDict.ContainsKey("components") && gameObjectDict["components"] is List<object> componentList){
                        foreach (object component in componentList){
                            if (component is Dictionary<string,object> componentDict){
                                if (componentDict.ContainsKey("properties") && componentDict["properties"] is Dictionary<string,object> propertiesDict)
                                {
                                    if (propertiesDict.ContainsKey("Name") && componentDict["type"] is string type)
                                    {
                                        string scriptName = propertiesDict["Name"].ToString();
                                        if (scriptName == boname && type == "Script")
                                        {
                                            string goname = gameObjectDict["name"].ToString();
                                            //Debug.Log("Le gameobject " + goname + " est ajouté au baseobject " + boname);
                                            list.Add(goname);
                                            

                                            
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return list;
    }

    private static List<Attribute> MapAttributes(Dictionary<string, object> jsonDict)
    {
        var attributesList = new List<Attribute>();

        if (jsonDict.ContainsKey("Attributes") && jsonDict["Attributes"] is List<object> attributes)
        {
            foreach (var attributeObj in attributes)
            {
                if (attributeObj is Dictionary<string, object> attributeDict)
                {
                    var attribute = new Attribute
                    {
                        Name = attributeDict.ContainsKey("Name") ? attributeDict["Name"].ToString() : string.Empty,
                        Type = attributeDict.ContainsKey("Type") ? attributeDict["Type"].ToString() : string.Empty,
                    };

                    attributesList.Add(attribute);
                }
            }
        }
        return attributesList;
    }

    private static List<Method> MapMethods(Dictionary<string, object> jsonDict)
    {
        var methodsList = new List<Method>();

        if (jsonDict.ContainsKey("Methods") && jsonDict["Methods"] is List<object> methods)
        {
            foreach (var methodObj in methods)
            {
                if (methodObj is Dictionary<string, object> methodDict)
                {
                    var method = new Method
                    {
                        Name = methodDict.ContainsKey("Name") ? methodDict["Name"].ToString() : string.Empty,
                        ReturnType = methodDict.ContainsKey("ReturnType") ? methodDict["ReturnType"].ToString() : string.Empty,
                        Parameters = MapAttributes(methodDict)
                    };

                    methodsList.Add(method);
                }
            }
        }

        return methodsList;
    }

    private static List<BaseObject> MapComposedClasses(Dictionary<string, object> jsonDict)
    {
        var composedClassesList = new List<BaseObject>();
        if (jsonDict.ContainsKey("ComposedClasses") && jsonDict["ComposedClasses"] is List<object> composedClasses)
        {
            foreach (var composedClass in composedClasses)
            {
                if (composedClass is string composedClassName)
                {
                    var composedClassObject = MapPreciseBaseObject(Classes, composedClassName);
                    if (composedClassObject == null){
                        continue;
                    }
                    composedClassesList.Add(composedClassObject);
                }
            }
        }

        return composedClassesList;
    }
}

}