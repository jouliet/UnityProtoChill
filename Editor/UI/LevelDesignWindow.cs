using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ChatGPTWrapper;
using System;
using System.IO;


public class LevelDesignWindow : EditorWindow
{
    private string levelDescription = "";
    private bool onIteration;

    public static void ShowWindow()
    {
        GetWindow<LevelDesignWindow>("LevelDesign");
    }

    private void OnGUI()
    {
        if (LevelDesignCreator.gosOnScene == null || LevelDesignCreator.gosOnScene.Count == 0){
            LevelDesignCreator.ReloadGOs();
        }
        if (File.Exists(LevelDesignCreator.LevelDesignJsonPath)){
            onIteration = true;
        }else{
            onIteration = false;
        }
        // SECTION DESCRIPTION
        GUILayout.BeginVertical("box");
        if (!onIteration){
            GUILayout.Label("Describe and create the level", EditorStyles.boldLabel);
        }else{
            GUILayout.Label("Describe and modify the level", EditorStyles.boldLabel);
        }
        
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;

         if (string.IsNullOrEmpty(levelDescription))
        {
            GUI.color = Color.gray; // Texte en gris
            levelDescription = EditorGUILayout.TextArea("Enter your level description here...", textAreaStyle, GUILayout.Height(50), GUILayout.ExpandHeight(true));
            GUI.color = Color.white; // Remettre la couleur normale

            // Pour éviter d’enregistrer le placeholder en tant que valeur réelle
            if (levelDescription == "Enter your level description here...")
                levelDescription = "";
        }
        else
        {
            levelDescription = EditorGUILayout.TextArea(levelDescription, textAreaStyle, GUILayout.Height(50), GUILayout.ExpandHeight(true));
        }
        
        GUILayout.Space(15);

        
        // Bouton LEVEL GENERATION
        if (GUILayout.Button("Push On Scene", GUILayout.Height(40)))
        {
            Debug.Log("Level Generation Running!");
            if (!onIteration){
                LevelGeneration(levelDescription);
            }else{
                LevelModification(levelDescription);
            }
        }
        
        if (GUILayout.Button("Reset LevelDesign", GUILayout.Height(20))){
            ResetLD();
        }
        
        GUILayout.EndVertical();

        GUILayout.Space(20);
    }

    private void LevelGeneration(string input){
        LevelDesignCreator.GenerateLevelDesign(input);
    }

    private void LevelModification(string input){
        LevelDesignCreator.GenerateLevelDesignModification(input);
    }

    private void ResetLD(){
        LevelDesignCreator.DeleteGeneratedGOs();
        
    }
}

