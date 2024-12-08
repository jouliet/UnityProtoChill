using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UMLClassDiag
{
    public class UMLDiagView : EditorWindow
    {
        private VisualTreeAsset umlVisualTree;
        private StyleSheet umlStyleSheet;

        private BaseObject rootObject;
        private float x=100f;
        private float y=0f;

        private Vector2 dragStart;
        private VisualElement canvas;

        public static void ShowDiagram(BaseObject root)
        {
            var window = GetWindow<UMLDiagView>("UML Diagram");
            window.rootObject = root;
            window.Repaint();
            window.Refresh();
        }

        private void OnEnable()
        {
            // Load Stylesheet
            umlVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UMLDiagram.uxml");
            umlStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UMLDiagram.uss");

            // Hand-tool navigation set up
            canvas = new VisualElement();
            canvas.style.position = Position.Absolute;
            canvas.style.left = 0;
            canvas.style.top = 0;
            canvas.style.width = 5000; // TODO: add dynamic size off canvas
            canvas.style.height = 5000;
            canvas.styleSheets.Add(umlStyleSheet);

            rootVisualElement.Clear();
            rootVisualElement.Add(canvas);

            EnableHandTool();

            if (rootObject != null)
            {
                DrawNode(rootObject, x, y);
            }

        }

        public void DrawNode(BaseObject root, float x, float y)
        {
            var nodeContainer = new VisualElement();
            nodeContainer.style.position = Position.Absolute;
            nodeContainer.style.left = x;
            nodeContainer.style.top = y;

            // Apply style to node
            var umlNode = umlVisualTree.CloneTree();

            umlNode.Q<Label>("base-object").text = root.Name;

            //Add attributes of BaseObject
            var attributesContainer = umlNode.Q<VisualElement>("attributes");
            foreach (var attribute in root.Attributes)
            {
                attributesContainer.Add(new Label($"{attribute.Name}: {attribute.Type}"));
            }
            //Add methods of BaseObject
            var methodsContainer = umlNode.Q<VisualElement>("methods");
            foreach (var method in root.Methods)
            {
                methodsContainer.Add(new Label($"{method.Name}(): {method.ReturnType}"));
            }

            nodeContainer.Add(umlNode);
            canvas.Add(nodeContainer);

            //Draw ComposedClasses
        }

        private void Refresh()
        {
            if (rootObject != null)
            {
                DrawNode(rootObject, x, y);
            }
        }

        // HAND TOOL NAVIGATION

        private void EnableHandTool()
        {
            canvas.RegisterCallback<MouseDownEvent>(OnMouseDown);
            canvas.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            canvas.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // Record the starting point of the drag
            if (evt.button == 0)
            {
                dragStart = evt.mousePosition;
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            // Adjust canvas position
            if (evt.pressedButtons == 1)
            {
                Vector2 currentMousePosition = evt.mousePosition;
                Vector2 delta = currentMousePosition - dragStart;

                // Adjust the canvas position
                canvas.style.left = canvas.resolvedStyle.left + delta.x;
                canvas.style.top = canvas.resolvedStyle.top + delta.y;

                dragStart = currentMousePosition; // Update drag start for smooth movement
                evt.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            // Stop dragging
            if (evt.button == 0)
            {
                evt.StopPropagation();
            }
        }


    }
}