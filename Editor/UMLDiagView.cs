using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UMLClassDiag
{
    public class UMLDiagView : EditorWindow
    {
        private Vector2 scrollPosition;
        private BaseObject rootObject;
        private VisualElement rootElement;

        public static void ShowDiagram(BaseObject root)
        {
            var window = GetWindow<UMLDiagView>("UML Diagram");
            window.rootObject = root;
            window.Repaint();
            window.Refresh();
        }

        private void OnEnable()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UMLDiagram.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UMLDiagram.uss");

            rootElement = visualTree.CloneTree();
            rootElement.styleSheets.Add(styleSheet);
            rootVisualElement.Clear();
            rootVisualElement.Add(rootElement);

            if (rootObject != null)
            {
                DrawNode(rootObject);
            }

        }

        public void DrawNode(BaseObject root)
        {
            rootElement.Q<Label>("base-object").text = root.Name;

            var attributesContainer = rootElement.Q<VisualElement>("attributes");
            foreach (var attribute in root.Attributes)
            {
                attributesContainer.Add(new Label($"{attribute.Name}: {attribute.Type}"));
            }

            var methodsContainer = rootElement.Q<VisualElement>("methods");
            foreach (var method in root.Methods)
            {
                methodsContainer.Add(new Label($"{method.Name}(): {method.ReturnType}"));
            }
        }

        private void Refresh()
        {
            if (rootElement != null && rootObject != null)
            {
                DrawNode(rootObject);
            }
        }

    }
}