using System;
using System.Collections.Generic;
using System.Collections;

using System.Text;
using UMLStructure;

namespace AttributNP{
     public class Attribut : Functions
    {
        public string Name { get; set; }
        public Class Type { get; }
        public Visibility Visibility { get; }
        public Class ClassFromList { get; }


        public Attribut(string name, Class type, Visibility visibility, Class classFromList = null)
        {
            if (!ClassManager.Instance.VerifyTypeByName(type.Name))
                throw new ArgumentException("Le type spécifié n'existe pas dans le ClassManager.");
            Name = name;
            Type = type;
            Visibility = visibility;
            ClassFromList = classFromList;
        }


        public override string ToString()
        {
            string str;
            str = $" {Visibility} {Type.Name} {Name}";
            return str;
        }
    }
}
