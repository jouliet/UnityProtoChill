using System;
using System.Collections.Generic;
using System.Collections;

using System.Text;
using AttributNP;
using MethodNP;


namespace UMLStructure{

    public class Functions
    {
        private string clsName;

        public bool isPrimitive(Class cls){
            if (cls == null){
                return false;
            }
            clsName = cls.Name;
            if (clsName != "void" && clsName != "Vector3" && clsName != "string" && clsName != "bool"  && !clsName.StartsWith("Dictionary<") ){
                return false;
            }
            return true;
        }

    }

    public struct Visibility
    {
        private readonly string visibilityType;
        private Visibility(string type) { visibilityType = type; }
        public static Visibility Public => new Visibility("public");
        public static Visibility Private => new Visibility("private");
        public static Visibility Protected => new Visibility("protected");
        public override string ToString() => visibilityType;
    }

    public static class PrimitiveType
    {
        public static Class Void => new Class("void");
        public static Class Vector3 => new Class("Vector3");
        public static Class String => new Class("string");
        public static Class Bool => new Class("bool");
        public static Class List(Class type) => new Class($"List<{type.Name}>");
        public static Class Dictionary(Class keyType, Class valueType) => new Class($"Dictionary<{keyType.Name}, {valueType.Name}>");
    }

    public class ClassManager
    {
        private static ClassManager _instance;
        // _types contains all types names: Primitive and created types
        private List<string> _types;
        // _classes contains all created classes
        private List<Class> _classes;
        //Constructeur
        private ClassManager() 
        {
             _types = new List<string>(); 
             _classes = new List<Class>(); 
        }

        // Initialisation de l'instance
        public static ClassManager Instance => _instance ??= new ClassManager();
        
        public void AddType(Class newType)
        {
            if (!_types.Contains(newType.Name)) 
                _types.Add(newType.Name);
        }

        public void AddClasses(Class newClass){
            _classes.Add(newClass);
        }


        public IReadOnlyList<string> Types => _types.AsReadOnly();
        public IReadOnlyList<Class> Classes => _classes.AsReadOnly();
        public bool VerifyTypeByName(string typeName) => _types.Contains(typeName);
    }

    public class Class : Functions
    {
        public string Name { get; set; }
        private List<Attribut> _attributes;
        private List<Method> _methods;
        public  List<Class> _composedClasses { get; }

        // private List<Class> Children;
        // private Class Parent;
        public Class(string name, List<Attribut> attributes = null, List<Method> methods = null, List<Class> composedClasses = null)
        {
            Name = name;
            _attributes = attributes ?? new List<Attribut>();
            _methods = methods ?? new List<Method>();
            _composedClasses = composedClasses ?? new List<Class>();
            ClassManager.Instance.AddType(this);

            if (!isPrimitive(this) && !Name.StartsWith("List<") ){
                ClassManager.Instance.AddClasses(this);
            }
        }

        public void AddAttribute(string name, Class type, Visibility visibility, Class classFromList = null)
        {
            if (!ClassManager.Instance.VerifyTypeByName(type.Name))
                throw new ArgumentException("Le type spécifié n'existe pas dans le ClassManager.");
            _attributes.Add(new Attribut(name, type, visibility, classFromList));

            if (!isPrimitive(type))
            {
                // Si le type est une liste
                if (type.Name.StartsWith("List<") && !isPrimitive(classFromList))
                {
                    //Console.WriteLine("classFromList: " + classFromList );
                    _composedClasses.Add(classFromList);
                }
                else
                {
                    // Ajoute le type s'il ne s'agit pas d'une liste
                    _composedClasses.Add(type);
                }
            }
        }

        public void AddMethod(string name, Class returnType, List<Class> arguments, Visibility visibility)
        {
            if (returnType != null && !ClassManager.Instance.VerifyTypeByName(returnType.Name))
                throw new ArgumentException("Le type de retour spécifié n'existe pas dans le ClassManager.");
            _methods.Add(new Method(name, returnType, arguments, visibility));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Class: {Name}");

            sb.AppendLine("Attributes:");
            foreach (var attr in _attributes)
            {
                if (attr != null)
                    sb.AppendLine($"- {attr}");
            }

            sb.AppendLine("Methods:");
            foreach (var method in _methods)
            {
                if (method != null)
                    sb.AppendLine($"- {method}");
            }

            sb.AppendLine("ComposedClasses:");
            foreach (var composedClasses in _composedClasses)
            {
                if (composedClasses != null)
                    sb.AppendLine($"- {composedClasses.Name}");
            }

            return sb.ToString();
        }

        public IReadOnlyList<Attribut> Attributes => _attributes.AsReadOnly();
        public IReadOnlyList<Method> Methods => _methods.AsReadOnly();
        //public IReadOnlyList<List<Class>> CComposedClasses => ComposedClasses.AsReadOnly();

        
    }
        
}

