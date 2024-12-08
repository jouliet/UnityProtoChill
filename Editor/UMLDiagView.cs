using UnityEditor;
using UnityEngine;

namespace UMLClassDiag
{
    public class UMLDiagView : EditorWindow
    {
        private Vector2 scrollPosition;
        private BaseObject rootObject;

        public static void ShowDiagram(BaseObject root)
        {
            var window = GetWindow<UMLDiagView>("UML Diagram");
            window.rootObject = root;
            window.Repaint();
        }

        private void OnGUI()
        {
            GUILayout.Label("UML Diagram", EditorStyles.boldLabel);

            if (rootObject == null)
            {
                GUILayout.Label("No UML data to display.", EditorStyles.helpBox);
                return;
            }

            DrawNode(rootObject);
        }

        public void DrawNode(BaseObject root)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(root.Name, EditorStyles.boldLabel);

            GUILayout.Space(10);

            foreach (var attribute in root.Attributes)
            {
                GUILayout.Label(attribute.Name);
            }

            GUILayout.Space(10);

            foreach (var method in root.Methods)
            {
                GUILayout.Label(method.Name);
            }

            GUILayout.EndVertical();
        }

    }
}