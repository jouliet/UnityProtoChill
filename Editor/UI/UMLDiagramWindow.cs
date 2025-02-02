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

        private List<BaseObject> baseObjects = new List<BaseObject>();
        private HashSet<BaseObject> drawnNodes = new HashSet<BaseObject>(); // Suivi des nœuds déjà dessinés
        private Dictionary<BaseObject, VisualElement> nodeElements = new Dictionary<BaseObject, VisualElement>(); // Associe chaque BaseObject à son élément visuel
        private List<Connection> connections = new List<Connection> ();

        private float width = 300f; // width of nodes
        private float offset = 50f;

        private Vector2 dragStart;
        private bool isDragging;
        private VisualElement canvas;

        private VisualElement loadingContainer;
        private Image loadingImage;
        private List<Texture2D> loadingImages = new List<Texture2D>();
        private bool isLoading = false;
        private int loadingImageIndex = 0;

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
                nodeElements.Clear();
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
            var windowStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UI/UMLWindow.uss");

            var root = windowVisualTree.CloneTree();
            root.styleSheets.Add(windowStyleSheet);
            rootVisualElement.Clear();
            rootVisualElement.Add(root);

            var overlayContainer = root.Q<VisualElement>("overlay-container");
            overlayContainer.pickingMode = PickingMode.Ignore;
            loadingContainer = root.Q<VisualElement>("loading-container");
            LoadImages();

            // Set up UML canvas
            canvas = root.Q<VisualElement>("canvas");
            canvas.styleSheets.Add(umlStyleSheet);
            canvas.style.flexGrow = 0;
            canvas.style.flexShrink = 0;

            canvas.schedule.Execute(() =>
            {
                DrawNetwork();
            }).ExecuteLater(100);


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
                    DrawNode(baseObject, currentX, currentY);
                    currentX += width + offset; // Avancer horizontalement pour le prochain nœud
                }
            }

            canvas.schedule.Execute(() =>
            {
                AdjustCanvasSize();
                DrawConnections();
            }).ExecuteLater(100);
        }

        public void DrawNode(BaseObject obj, float x, float y)
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

            var generateButton = new Button(() => OnGenerateObject(obj)) { text = "Generate" };
            umlNode.Add(generateButton);
            var collapseButton = umlNode.Q<Button>("collapse-button");
            var collapseContent = umlNode.Q<VisualElement>("uml-diagram-contents");
            collapseButton.clicked +=  () => OnCollapseNode(obj, nodeContainer, collapseContent, collapseButton);

            nodeContainer.Add(umlNode);

            // Add selection functionnality to node
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
                    DrawNode(child, childX, childY);
                    childX += width + offset; // Avancer horizontalement pour chaque enfant
                }
            }
        }

        public void DrawConnections()
        {
            if (nodeElements == null)
            {
                Debug.LogWarning("La liste nodeElements est null donc impossible de dessiner les liens.");
                return;
            }
            foreach (var kvp in nodeElements)
            {
                var parentObject = kvp.Key;
                var parentNode = kvp.Value;

                foreach (var child in parentObject.ComposedClasses)
                {
                    if (nodeElements.TryGetValue(child, out var childNode))
                    {
                        List<Vector2> parentAnchors = FindAnchorPoints(parentNode);
                        List<Vector2> childAnchors = FindAnchorPoints(childNode);
                        List<Vector2> lineCoordinates = FindBestConnection(parentAnchors, childAnchors);

                        var line = DrawLine(lineCoordinates[1].x, lineCoordinates[1].y, lineCoordinates[0].x, lineCoordinates[0].y);
                        connections.Add(new Connection(line, parentObject, child));
                    }
                }
            }
        }

        private List<BaseObject> ClearConnections(BaseObject obj)
        {
            var connectionsToClear = new List<Connection>();
            var parents = new List<BaseObject>();

            foreach (var connection in connections)
            {
                if (connection.Start == obj)
                {
                    connection.Arrow.RemoveFromHierarchy();
                    connectionsToClear.Add(connection);
                }
                if (connection.End == obj)
                {
                    connection.Arrow.RemoveFromHierarchy();
                    connectionsToClear.Add(connection);
                    parents.Add(connection.Start);
                }
            }
            foreach (var connection in connectionsToClear)
            {
                connections.Remove(connection);
            }

            return parents;
        }

        private VisualElement DrawLine(float endpointX, float endpointY, float originX, float originY)
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

            var arrowPointer = new VisualElement();
            arrowPointer.AddToClassList("uml-arrow");
            arrowPointer.style.left = endpointX - 10f;
            arrowPointer.style.top = endpointY - 10f;
            arrowPointer.style.rotate = new StyleRotate(new Rotate(angle - 90));

            var arrow = new VisualElement();
            arrow.Add(line);
            arrow.Add(arrowPointer);
            canvas.Add(arrow);

            return arrow;
        }

        private List<Vector2> FindAnchorPoints(VisualElement node)
        {
            List<Vector2> anchorPoints = new List<Vector2>();

            // top anchor point
            anchorPoints.Add(new Vector2(node.resolvedStyle.left + width / 2, node.resolvedStyle.top));
            // left anchor point
            anchorPoints.Add(new Vector2(node.resolvedStyle.left, node.resolvedStyle.top + node.resolvedStyle.height / 2));
            // right anchor point
            anchorPoints.Add(new Vector2(node.resolvedStyle.left + width, node.resolvedStyle.top + node.resolvedStyle.height / 2));
            // bottom anchor point
            anchorPoints.Add(new Vector2(node.resolvedStyle.left + width / 2, node.resolvedStyle.top + node.resolvedStyle.height));

            return anchorPoints;
        }

        private List<Vector2> FindBestConnection(List<Vector2> anchorsParent, List<Vector2> anchorsChild)
        {
            List<Vector2> anchors = new List<Vector2> { Vector2.zero, Vector2.zero };

            float length = float.MaxValue;
            var center = new Vector2(anchorsChild[0].x, anchorsChild[2].y);

            for (int i = 0; i < anchorsParent.Count; i++)
            {
                float dx = center.x - anchorsParent[i].x;
                float dy = center.y - anchorsParent[i].y;
                if (length > Mathf.Sqrt(dx * dx + dy * dy))
                {
                    length = Mathf.Sqrt(dx * dx + dy * dy);
                    anchors[0] = anchorsParent[i];
                }
            }

            length = float.MaxValue;
            for (int i = 0; i < anchorsChild.Count; i++)
            {
                float dx = anchorsChild[i].x - anchors[0].x;
                float dy = anchorsChild[i].y - anchors[0].y;
                if (length > Mathf.Sqrt(dx * dx + dy * dy))
                {
                    length = Mathf.Sqrt(dx * dx + dy * dy);
                    anchors[1] = anchorsChild[i];
                }
            }

            return anchors;
        }

        private Rect CalculateBoundingBox()
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var kvp in nodeElements)
            {
                var node = kvp.Value;

                float nodeLeft = node.resolvedStyle.left;
                float nodeTop = node.resolvedStyle.top;
                float nodeHeight = node.resolvedStyle.height;

                minX = Mathf.Min(minX, nodeLeft);
                minY = Mathf.Min(minY, nodeTop);
                maxX = Mathf.Max(maxX, nodeLeft + width);
                maxY = Mathf.Max(maxY, nodeTop + nodeHeight);
            }

            float padding = 100f;
            return new Rect(minX - padding, minY - padding, maxX - minX + 2 * padding, maxY - minY + 2 * padding);
        }

        private void AdjustCanvasSize()
        {
            Rect boundingBox = CalculateBoundingBox();

            canvas.style.width = boundingBox.width;
            canvas.style.height = boundingBox.height;

            canvas.style.left = -boundingBox.x;
            canvas.style.top = -boundingBox.y;
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
        private const float minZoom = 0.1f;
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
            ReloadDiagram(baseObjects);
            /*var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(UMLFilePath);
            if (jsonFile != null)
            {
                string jsonString = jsonFile.text;
                Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
                ObjectResearch.CleanUp();
                List<BaseObject> baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
                GenerativeProcess.SetJsonScripts(jsonString);
                ReloadDiagram(baseObjects);
            }*/
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

        /// 
        /// ANIMATIONS
        /// 
        ///
        private void OnCollapseNode(BaseObject obj, VisualElement objContainer, VisualElement collapseContainer, Button collapseButton)
        {
            if (collapseContainer.style.display == DisplayStyle.Flex)
            {
                // Collapse the node
                collapseContainer.style.display = DisplayStyle.None;
                collapseButton.RemoveFromClassList("collapse-button__down");
                collapseButton.AddToClassList("collapse-button__up");
            }
            else
            {
                // Expand the node
                collapseContainer.style.display = DisplayStyle.Flex;
                collapseButton.RemoveFromClassList("collapse-button__up");
                collapseButton.AddToClassList("collapse-button__down");
            }

            var parents = ClearConnections(obj);

            objContainer.schedule.Execute(() =>
            {
                foreach (var child in obj.ComposedClasses)
                {
                    if (nodeElements.TryGetValue(child, out var childNode))
                    {
                        List<Vector2> parentAnchors = FindAnchorPoints(objContainer);
                        List<Vector2> childAnchors = FindAnchorPoints(childNode);
                        List<Vector2> lineCoordinates = FindBestConnection(parentAnchors, childAnchors);

                        var line = DrawLine(lineCoordinates[1].x, lineCoordinates[1].y, lineCoordinates[0].x, lineCoordinates[0].y);

                        connections.Add( new Connection(line, obj, child) );
                    }
                }
                foreach (var parent in parents)
                {
                    if (nodeElements.TryGetValue(parent, out var parentNode))
                    {
                        List<Vector2> parentAnchors = FindAnchorPoints(parentNode);
                        List<Vector2> childAnchors = FindAnchorPoints(objContainer);
                        List<Vector2> lineCoordinates = FindBestConnection(parentAnchors, childAnchors);

                        var line = DrawLine(lineCoordinates[1].x, lineCoordinates[1].y, lineCoordinates[0].x, lineCoordinates[0].y);

                        connections.Add(new Connection(line, parent, obj) );
                    }
                }

                canvas.MarkDirtyRepaint();
            });
        }

        private void OnGenerateObject(BaseObject obj)
        {
            Debug.Log($"GenerateObject called for {obj.Name}");

            obj.GenerateScript();
        }

        public void OnLoadingUML(bool state)
        {
            canvas.Clear();

            if (loadingImage == null)
            {
                Debug.LogError("loadingImage is null. Ensure LoadImages() is called.");
                return;
            }

            if (state)
            {
                var loadingText = new Label("GENERATING UML...");
                loadingText.style.top = 0f;
                loadingText.style.left = 0f;
                loadingText.style.fontSize = 20f;

                loadingContainer.Add(loadingText);
                loadingContainer.Add(loadingImage);

                isLoading = true;
                UpdateLoadingAnimation(0.5f);
            }
            else
            {
                loadingImageIndex = 0;
                loadingImage.image = loadingImages[loadingImageIndex];
                isLoading = false;
                loadingContainer.Clear();
            }
        }

        private void UpdateLoadingAnimation(float waitTime)
        {
            if (!isLoading)
            {
                return;
            }

            if (loadingImages.Count > 0)
            {
                loadingImage.image = loadingImages[loadingImageIndex];
                loadingImageIndex = (loadingImageIndex + 1) % 6;
            }

            loadingContainer.schedule.Execute(() => UpdateLoadingAnimation(waitTime)).StartingIn((long)(waitTime * 1000));
        }

        private void LoadImages()
        {
            for (int i = 1; i <= 6; i++)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Packages/com.jin.protochill/Editor/UI/resources/loading-{i}.png");
                if (texture != null)
                {
                    loadingImages.Add(texture);
                }
                else
                {
                    Debug.LogError($"Failed to load image: loading-{i}.png");
                }
            }

            loadingImage = new Image();
            loadingImage.style.width = 100;
            loadingImage.style.height = 100;
            loadingImage.image = loadingImages[0];
        }
    }

    public struct Connection
    {
        public VisualElement Arrow { get; }
        public BaseObject Start { get; }
        public BaseObject End { get; }

        public Connection(VisualElement arrow, BaseObject start, BaseObject end)
        {
            Arrow = arrow;
            Start = start;
            End = end;
        }
    }
}