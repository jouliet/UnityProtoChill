using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static SaverLoader;

namespace UMLClassDiag
{
    public class UMLDiagramWindow : EditorWindow
    {
        private VisualTreeAsset umlVisualTree;
        private StyleSheet umlStyleSheet;

        private BaseObject rootObject;
        private float width = 300f;
        private float offset = 10f;

        private Vector2 dragStart;
        private VisualElement canvas;
        private float canvasWidth = 1000f;
        private float canvasHeight = 1000f;

        public static void ShowDiagram(BaseObject root)
        {
            var window = GetWindow<UMLDiagramWindow>("UML Diagram");
            window.rootObject = root;
            window.Repaint();
            window.Refresh();
        }
        private void OnGUI()
        {   

            if (ObjectResearch.AllBaseObjects.Count == 0){
                LoadUML();
            }
        }
        public void ReloadDiagram(BaseObject root)
        {
            canvas.Clear();
            if (root == null)
            {
                Debug.LogError("Root object is null in Reaload Diagram.");
            }
            else
            {
                this.rootObject = root;
                DrawNode(rootObject, canvasWidth / 2, 0f);
            }
        }

        public VisualElement CreateDiagramView()
        {
            // Load Stylesheet
            umlVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/UMLDiagram.uxml");
            umlStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UI/UMLDiagram.uss");

            // Hand-tool navigation set up
            canvas = new VisualElement();
            canvas.style.position = Position.Absolute;
            canvas.style.left = - canvasWidth / 2 + width; // Center display
            canvas.style.top = 0f;
            canvas.style.width = canvasWidth; // TODO: add dynamic size off canvas
            canvas.style.height = canvasHeight;
            canvas.styleSheets.Add(umlStyleSheet);

            rootVisualElement.Clear();
            rootVisualElement.Add(canvas);

            if (rootObject != null)
            {
                DrawNode(rootObject, canvasWidth / 2, 0f);
            }

            EnableHandTool();

            return canvas;
        }

        //
        // DRAWING
        //

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

            var generateButton = new Button(() => GenerateObject(root)) { text = "Generate" };
            umlNode.Add(generateButton);

            nodeContainer.Add(umlNode);
            canvas.Add(nodeContainer);

            nodeContainer.schedule.Execute(() =>
            {
                float height = nodeContainer.resolvedStyle.height;

                //Draw ComposedClasses
                if (root.ComposedClasses.Count > 0)
                {
                    float totalChildWidth = 0;
                    float[] childWidths = new float[root.ComposedClasses.Count];

                    for (int i = 0; i < root.ComposedClasses.Count; i++)
                    {
                        childWidths[i] = CalculateTotalWidth(root.ComposedClasses[i]);
                        totalChildWidth += childWidths[i];
                    }
                    totalChildWidth += (root.ComposedClasses.Count - 1) * offset;

                    int count = 0;
                    float startX = x + (width - totalChildWidth) / 2;
                    float currentX = startX;
                    foreach (var baseObject in root.ComposedClasses)
                    {
                        float currentY = y + height + offset;
                        DrawNode(baseObject, currentX, currentY);
                        DrawLine(currentX + width / 2, currentY + offset, x + width / 2, y +  height - offset);
                        currentX += childWidths[count] + offset;
                        count++;
                    }
                }

            });
        }
        public void GenerateObject(BaseObject obj)
        {
            // Ajoutez la logique pour générer un objet à partir de `obj`.
            Debug.Log($"GenerateObject called for {obj.Name}");

            // Exemple d'appel d'une méthode `Generate` sur l'objet
            obj.GenerateScript();  // Si vous avez une méthode `Generate` sur votre classe `BaseObject`
        }
        private float CalculateTotalWidth(BaseObject node)
        {
            if (node.ComposedClasses.Count == 0)
            {
                return width;
            }

            float totalWidth = 0;
            foreach (var baseObject in node.ComposedClasses)
            {
                totalWidth += CalculateTotalWidth(baseObject) + offset;
            }

            return totalWidth - offset;
        }

        private void DrawLine(float endpointX, float endpointY, float originX, float originY)
        {
            float dx = endpointX - originX;
            float dy = endpointY - originY;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

            var line = new VisualElement();
            line.AddToClassList("uml-line");
            line.style.left = originX;
            line.style.top = originY;
            line.style.width = length;
            line.style.rotate = new StyleRotate(new Rotate(angle));

            canvas.Add(line);
        }

        private void Refresh()
        {
            if (rootObject != null)
            {
                DrawNode(rootObject, canvasWidth / 2, 0f);
            }
        }

        //
        // HAND TOOL NAVIGATION
        //
        private void EnableHandTool()
        {
            canvas.RegisterCallback<MouseDownEvent>(OnMouseDown);
            canvas.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            canvas.RegisterCallback<MouseUpEvent>(OnMouseUp);
            canvas.RegisterCallback<WheelEvent>(OnMouseWheel);
            AddZoomButtons();
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

        //
        // ZOOM TOOL
        //
        private float zoomScale = 1.0f;
        private const float zoomIncrement = 0.1f;
        private const float minZoom = 0.5f;
        private const float maxZoom = 2.0f;

        private void OnMouseWheel(WheelEvent evt)
        {
            Vector2 mousePosition = evt.mousePosition;
            Vector2 localMousePosition = mousePosition - canvas.worldBound.position;

            // Calculate the scale before changing it
            float oldScale = zoomScale;

            // Adjust zoom scale based on scroll direction
            if (evt.delta.y < 0)
            {
                zoomScale = Mathf.Min(zoomScale + zoomIncrement, maxZoom);
            }
            else
            {
                zoomScale = Mathf.Max(zoomScale - zoomIncrement, minZoom);
            }

            // Calculate the scaling factor
            float scaleFactor = zoomScale / oldScale;

            // Adjust canvas position to keep the zoom centered on the mouse position
            Vector2 adjustment = localMousePosition * (1 - scaleFactor);
            canvas.style.left = canvas.resolvedStyle.left + adjustment.x;
            canvas.style.top = canvas.resolvedStyle.top + adjustment.y;

            UpdateCanvasScale();
            evt.StopPropagation();
        }

        private void AddZoomButtons()
        {
            var overlayContainer = new VisualElement();
            overlayContainer.style.position = Position.Absolute;
            overlayContainer.style.top = 0;
            overlayContainer.style.left = 0;
            overlayContainer.style.right = 0;
            overlayContainer.style.bottom = 0;
            rootVisualElement.Add(overlayContainer);

            var zoomInButton = new Button(() => ChangeZoom(true)) { text = "+" };
            var zoomOutButton = new Button(() => ChangeZoom(false)) { text = "-" };

            zoomInButton.style.position = Position.Absolute;
            zoomInButton.style.top = 10;
            zoomInButton.style.left = 10;

            zoomOutButton.style.position = Position.Absolute;
            zoomOutButton.style.top = 50;
            zoomOutButton.style.left = 10;

            overlayContainer.Add(zoomInButton);
            overlayContainer.Add(zoomOutButton);
        }

        private void ChangeZoom(bool zoomIn)
        {
            if (zoomIn)
            {
                zoomScale = Mathf.Min(zoomScale + zoomIncrement, maxZoom);
            }
            else
            {
                zoomScale = Mathf.Max(zoomScale - zoomIncrement, minZoom);
            }

            UpdateCanvasScale();
        }

        private void UpdateCanvasScale()
        {
            canvas.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(50, 50, 0));
            canvas.style.scale = new Scale(new Vector3(zoomScale, zoomScale, 1));
        }

    }
}