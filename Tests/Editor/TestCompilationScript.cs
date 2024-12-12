
using UnityEngine;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
public class TestCompilationScript
{
    [Test]
    public void TestWriteScriptFile()
        {
            // Chemin du dossier et du fichier
            string filename = "player.cs";
            string folderPath = "Assets/Scripts";
            string filePath = Path.Combine(folderPath, filename);
            string content = "ce script ne compile pas pour des raisons évidentes";
            // Vérifie si le dossier existe, sinon le crée

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log("Dossier créé : " + folderPath);
            }


            File.WriteAllText(filePath, content);
            Debug.Log("Fichier créé : " + filePath);
        }
}
