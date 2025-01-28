using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using static JsonParser;
using static UIManager;
using static SaverLoader;

namespace UMLClassDiag
{
    public class UMLDiagramWindow : EditorWindow
    {
        private UIManager uiManager;

        private VisualTreeAsset umlVisualTree;
        private StyleSheet umlStyleSheet;

        private BaseObject rootObject;
        private List<BaseObject> baseObjects = new List<BaseObject>();
        private HashSet<BaseObject> drawnNodes = new HashSet<BaseObject>(); // Suivi des nœuds déjà dessinés
        private Dictionary<BaseObject, VisualElement> nodeElements = new Dictionary<BaseObject, VisualElement>(); // Associe chaque BaseObject à son élément visuel

        private float width = 300f;
        private float offset = 50f;

        private Vector2 dragStart;
        private bool isDragging;
        private VisualElement canvas;
        private float canvasWidth = 1000f;
        // private float canvasHeight = 1000f;

        private VisualElement selectedNode;
        private BaseObject selectedObject;

        public static void ShowDiagram(List<BaseObject> baseObjects){
            var window = GetWindow<UMLDiagramWindow>("UML Diagram");
            window.baseObjects = baseObjects;
            window.Repaint();
            window.Refresh();
        }

        private void OnGUI()
        {   
            if (ObjectResearch.AllBaseObjects.Count == 0){
                LoadUML();
            }
        }
        public void ReloadDiagram(List<BaseObject> baseObjects)
        {
            canvas.Clear();
            if (baseObjects == null)
            {
                Debug.LogError("Root object is null in Reaload Diagram.");
            }
            else
            {
                this.baseObjects = baseObjects;
                // DrawNode(rootObject, canvasWidth / 2, 0f);
                DrawNetwork();
            }
        }

        public VisualElement CreateDiagramView(UIManager manager)
        {
            uiManager = manager;

            // Load Stylesheet
            umlVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/UMLDiagram.uxml");
            umlStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UI/UMLDiagram.uss");
            var windowVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/UMLWindow.uxml");

            var root = windowVisualTree.CloneTree();
            rootVisualElement.Clear();
            rootVisualElement.Add(root);

            // Set up UML canvas
            canvas = root.Q<VisualElement>("canvas");
            //canvas.style.left = - canvasWidth / 2 + width; // Center display
            //canvas.style.top = 0f;
            //canvas.style.width = canvasWidth; // TODO: add dynamic size off canvas
            //canvas.style.height = canvasHeight;
            canvas.styleSheets.Add(umlStyleSheet);

            // if (rootObject != null)
            // {
            //     DrawNode(rootObject, canvasWidth / 2, 0f);
            // }
            DrawNetwork();


            // Zoom buttons set up
            var zoomInButton = root.Q<Button>("zoom-in-button");
            zoomInButton.clicked += () => ChangeZoom(true);
            var zoomOutButton = root.Q<Button>("zoom-out-button");
            zoomOutButton.clicked += () => ChangeZoom(false);
            var refreshButton = root.Q<Button>("refresh-button");
            refreshButton.clicked += OnRefreshtButtonClick;

            EnableHandTool();

            return root;
        }


        //
        // DESSIN DU RÉSEAU
        //
        private void DrawNetwork()
        {
            float currentX = 50f; // Position initiale pour les nœuds
            float currentY = 50f;
            drawnNodes.Clear();
            
            foreach (var baseObject in baseObjects)
            {
                if (!drawnNodes.Contains(baseObject))
                {
                    _DrawNode(baseObject, currentX, currentY);
                    currentX += width + offset; // Avancer horizontalement pour le prochain nœud
                }
            }
            DrawConnections();
        }

        public void _DrawNode(BaseObject obj, float x, float y)
        {
            if (drawnNodes.Contains(obj)) return; // Ne pas redessiner un nœud déjà affiché

            var nodeContainer = new VisualElement();
            nodeContainer.style.position = Position.Absolute;
            nodeContainer.style.left = x;
            nodeContainer.style.top = y;

            // Créer et configurer le nœud
            var umlNode = umlVisualTree.CloneTree();
            umlNode.Q<Label>("base-object").text = obj.Name;

            // Ajouter les attributs
            var attributesContainer = umlNode.Q<VisualElement>("attributes");
            foreach (var attribute in obj.Attributes)
            {
                attributesContainer.Add(new Label($"{attribute.Name}: {attribute.Type}"));
            }

            // Ajouter les méthodes
            var methodsContainer = umlNode.Q<VisualElement>("methods");
            foreach (var method in obj.Methods)
            {
                methodsContainer.Add(new Label($"{method.Name}(): {method.ReturnType}"));
            }

            // Bouton d'action
            var generateButton = new Button(() => GenerateObject(obj)) { text = "Generate" };
            umlNode.Add(generateButton);

            nodeContainer.Add(umlNode);

            nodeContainer.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (!isDragging) {
                    if (selectedObject != null && selectedObject != obj)
                    {
                        selectedNode.RemoveFromClassList("uml-diagram__selected");
                    }
                    if (nodeContainer.ClassListContains("uml-diagram__selected"))
                    {
                        nodeContainer.RemoveFromClassList("uml-diagram__selected");
                        selectedNode = null;
                        selectedObject = null;
                    }
                    else
                    {
                        nodeContainer.AddToClassList("uml-diagram__selected");
                        selectedNode = nodeContainer;
                        OnSelectNode(obj);
                        
                    }
                }
                evt.StopPropagation();
            });
            canvas.Add(nodeContainer);
            
            // Mémoriser le nœud dessiné
            drawnNodes.Add(obj);
            nodeElements[obj] = nodeContainer;
            
            // Dessiner les connexions vers les ComposedClasses
            float childY = y + 300f; // Position verticale pour les enfants
            float childX = x; // Position horizontale initiale pour les enfants
            foreach (var child in obj.ComposedClasses)
            {
                
                if (!drawnNodes.Contains(child))
                {
                    _DrawNode(child, childX, childY);
                    childX += width + offset; // Avancer horizontalement pour chaque enfant
                }
            }


             // Planifier les connexions après le rendu
            nodeContainer.schedule.Execute(() =>
            {
                DrawConnections();
            });
        }
        public void DrawConnections()
        {
            if (nodeElements == null)
            {
                Debug.LogWarning("La liste nodeElements est null donc impossible de dessiner les liens.");
            }
            foreach (var kvp in nodeElements)
            {
                var parentObject = kvp.Key;
                var parentNode = kvp.Value;

                foreach (var child in parentObject.ComposedClasses)
                {
                    if (nodeElements.TryGetValue(child, out var childNode))
                    {
                        // Récupérer les positions et dimensions directement des nœuds
                        float parentX = parentNode.resolvedStyle.left + parentNode.resolvedStyle.width / 2;
                        float parentY = parentNode.resolvedStyle.top + parentNode.resolvedStyle.height;

                        float childX = childNode.resolvedStyle.left + childNode.resolvedStyle.width / 2;
                        float childY = childNode.resolvedStyle.top;

                        
                        // Dessiner une ligne entre le parent et l'enfant
                        DrawLine(childX, childY, parentX, parentY);
                    }
                }
            }
        }
        public void GenerateObject(BaseObject obj)
        {
            // Ajoutez la logique pour g�n�rer un objet � partir de `obj`.
            Debug.Log($"GenerateObject called for {obj.Name}");

            // Exemple d'appel d'une m�thode `Generate` sur l'objet
            obj.GenerateScript();  // Si vous avez une m�thode `Generate` sur votre classe `BaseObject`
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
            if (baseObjects != null)
            {
                DrawNetwork();
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
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // Record the starting point of the drag
            if (evt.button == 0) // left click event
            {
                dragStart = evt.mousePosition;
                isDragging = false;

                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            // Adjust canvas position
            if (evt.pressedButtons == 1) // for left mouse button
            {
                isDragging = true;

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
            if (evt.button == 0)
            {
                // handles deselected when clicking on canvas
                if (!isDragging)
                {
                    if (selectedNode != null)
                    {
                        selectedNode.RemoveFromClassList("uml-diagram__selected");
                        selectedNode = null;
                        selectedObject = null;
                    }
                }
                isDragging = false;

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

        /// 
        /// CLICK EVENTS
        /// 
        private void OnRefreshtButtonClick()
        {
            LoadUML();
            var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(UMLFilePath);
            if (jsonFile != null)
            {
                string jsonString = jsonFile.text;
                Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
                ObjectResearch.CleanUp();
                List<BaseObject> baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
                GenerativeProcess.SetJsonScripts(jsonString);
                ReloadDiagram(baseObjects);
            }
        }
        
        /// 
        /// BASE OBJECT SELECTION
        /// 
        ///
        private void OnSelectNode(BaseObject baseObject)
        {
            selectedObject = baseObject;
            string msg = $"Node {baseObject.Name} selected in UML Diagram.";
            uiManager.SendMessageToChatWindow(msg);
        }

        public BaseObject GetSelectedBaseObjectFromUML()
        {
            return selectedObject;
        }
    }
}