using System;
using System.Collections.Generic;
using System.Collections;

using System.Text;  
using UMLStructure;

namespace MethodNP{
  public class Method
    {
        public string Name { get; set; }
        public Class ReturnType { get; }
        private List<Class> _arguments;
        public Visibility Visibility { get; }

        public Method(string name, Class returnType, List<Class> arguments, Visibility visibility)
        {
            if (returnType != null && !ClassManager.Instance.VerifyTypeByName(returnType.Name))
                throw new ArgumentException("Le type de retour spécifié n'existe pas dans le ClassManager.");
            Name = name;
            ReturnType = returnType;
            _arguments = arguments ?? new List<Class>();
            Visibility = visibility;
        }

        public override string ToString()
        {
            string str;
            str = $" {Visibility} {ReturnType?.Name ?? "void"} {Name}";
            return str;
        }

        public IReadOnlyList<Class> Arguments => _arguments.AsReadOnly();
    }
}