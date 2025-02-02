using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ChatGPTWrapper;
using System;


public class LevelDesignWindow : EditorWindow
{
    private string levelDescription = "";

    public static void ShowWindow()
    {
        GetWindow<LevelDesignWindow>("LevelDesign");
    }

    private void OnGUI()
    {
        // SECTION DESCRIPTION
        GUILayout.BeginVertical("box");
        GUILayout.Label("Describe the level", EditorStyles.boldLabel);
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        levelDescription = EditorGUILayout.TextArea("I want a platformer leveldesign that obliges me to jump to get to the end of the level.", textAreaStyle, GUILayout.Height(50), GUILayout.ExpandHeight(true));
        
        GUILayout.Space(15);

        // Bouton LEVEL GENERATION
        if (GUILayout.Button("Push On Scene", GUILayout.Height(40)))
        {
            LevelGeneration(levelDescription);
        }
        GUILayout.EndVertical();

        GUILayout.Space(20);
    }

    private void LevelGeneration(string input){
        Debug.Log("Level Generation Running!");
        LevelDesignCreator.GenerateLevelDesign(input);
    }
}

