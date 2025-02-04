using UnityEngine;
using System.Collections.Generic;
using UMLClassDiag;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using static JsonParser;
using static UIManager;
using static SaverLoader;
using static ObjectResearch;
using System;
using static ScriptsCoherenceHandler;
using System.Reflection; 
public class PrefabCoherenceHandler : MonoBehaviour
{
    // Call this method to load all prefabs and extract their components
    public static void UpdateBaseGameObjectsToMatchUnityProject()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        foreach (GameObject prefab in prefabs)
        {
            BaseGameObject bgo = ObjectResearch.FindBaseGameObjectByName(prefab.name);

            if (bgo == null){
                BaseGameObject baseGameObject = new BaseGameObject (prefab.name)
                {
                    Tag = prefab.tag,
                    Layer = LayerMask.LayerToName(prefab.layer)
                };

                ExtractScriptComponents(prefab, baseGameObject);
            }
            else {
                bgo.Tag = prefab.tag;
                bgo.Layer = LayerMask.LayerToName(prefab.layer);
                ExtractScriptComponents(prefab,bgo);
            }
        }
    }


    private static void ExtractScriptComponents(GameObject prefab, BaseGameObject baseGameObject)
    {
        Component[] components = prefab.GetComponents<Component>();

        foreach (Component component in components)
        {
            BaseObject baseComponent = new BaseObject(component.GetType().Name, true);

            // Create the dictionary dynamically for the component's properties
            //Dictionary<string, object> propertiesDictionary = ReadPropertiesFromComponent(component);

            

            // Initialize BaseObject with dynamically created dictionary
            // baseComponent.InitWithProperties(propertiesDictionary);
            baseGameObject.Components.Add(baseComponent);
        }
    }

}
// public static Dictionary<string, object> ReadPropertiesFromComponent(Component component)
// {
//     Dictionary<string, object> propertiesDict = new Dictionary<string, object>();

//     Type componentType = component.GetType();
//     PropertyInfo[] properties = componentType.GetProperties();

//     foreach (var property in properties)
//     {
//         try
//         {
//             // Skip indexed properties (like array or list indexers)
//             if (property.GetIndexParameters().Length > 0)
//             {
//                 Debug.LogWarning($"Skipping indexed property: {property.Name}");
//                 continue;
//             }

//             object value = null;

//             try
//             {
//                 value = property.GetValue(component);
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogWarning($"Error reading property {property.Name} of {componentType.Name}: {ex.Message}");
//                 continue; // Continue if unable to get value for this property
//             }

//             // If value is null, skip it
//             if (value == null)
//                 continue;
//             else if (component is MeshFilter meshFilter && property.Name == "sharedMesh")
//             {
//                 value = meshFilter.sharedMesh != null ? meshFilter.sharedMesh.name : "None";
//             }
//             else if (component is MeshRenderer meshRenderer && property.Name == "sharedMaterial")
//             {
//                 value = meshRenderer.sharedMaterial != null ? meshRenderer.sharedMaterial.name : "None";
//             }
//             else
//             {
//                 value = value.ToString();
//             }

//             // Add to dictionary
//             propertiesDict[property.Name] = value;
//         }
//         catch (Exception ex)
//         {
//             Debug.LogWarning($"Exception while processing property {property.Name} from {component.GetType().Name}: {ex.Message}");
//         }
//     }

//     return propertiesDict;
// }

// }
