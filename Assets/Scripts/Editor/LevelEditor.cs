﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    #region - Fields -
    private readonly string levelElementsPropertyName = nameof(Level.levelElements);
    private readonly string wavePropertyName = nameof(LevelElement.wave);
    private readonly string waveCountPropertyName = nameof(LevelElement.waveCount);
    private readonly string startEnergyPropertyName = nameof(Level.startEnergy);
    private readonly string startGoldPropertyName = nameof(Level.startGold);

    private Level level;
    private SerializedProperty levelElementsProperty;
    private SerializedProperty waveProperty;
    private SerializedProperty waveCountProperty;
    private SerializedProperty startEnergyProperty;
    private SerializedProperty startGoldProperty;
    private ReorderableList reorderableList;
    private int indexOfElementToDelete = -1;

    // Validation circles
    private Texture greenCircle;
    private Texture redCircle;
    private GUIContent greenCircleContent;  // Used to signal something is valid
    private GUIContent redCircleContent;    // Used to signal something is invalid
    private bool hasValidationCircles;

    // GUI styles
    private GUIStyle labelHeaderStyle;
    #endregion

    #region - Public methods (OnInspectorGUI) -
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // LevelElements's "header"
        EditorGUILayout.LabelField("List of waves in this level:", labelHeaderStyle);
        EditorGUILayout.Space();

        DrawAddButtons();

        // Draws level elements - which hold information about waves used in the level
        reorderableList.DoLayoutList();
        EditorGUILayout.Space();

        DrawLevelValidityInfo();
        DrawStartEnergyProperty();
        DrawStartGoldProperty();

        if (indexOfElementToDelete != -1)
        {
            RemoveElementFromList();
        }

        serializedObject.ApplyModifiedProperties();
    } 
    #endregion

    #region - Private methods -
    private void OnEnable()
    {
        // Validation circles 
        greenCircle = AssetDatabase.LoadAssetAtPath(Constants.TexturesGreenCirclePath, typeof(Texture)) as Texture; 
        redCircle = AssetDatabase.LoadAssetAtPath(Constants.TexturesRedCirclePath, typeof(Texture)) as Texture; 
        if (greenCircle != null && redCircle != null)
        {
            greenCircleContent = new GUIContent(greenCircle);
            redCircleContent = new GUIContent(redCircle);
            hasValidationCircles = true;
        }

        // Target and its properties 
        level = (Level)target;
        levelElementsProperty = serializedObject.FindProperty(levelElementsPropertyName);

        // GUI styles
        labelHeaderStyle = new GUIStyle();
        labelHeaderStyle.fontSize = 15;

        // Reorderable list of level elements
        InitializeReorderableList();

        // Energy and start gold
        startEnergyProperty = serializedObject.FindProperty(startEnergyPropertyName);
        startGoldProperty = serializedObject.FindProperty(startGoldPropertyName);
    }
    
    #region - Level elements related methods -
    private void InitializeReorderableList()
    {
        reorderableList = new ReorderableList(serializedObject, levelElementsProperty, true, true, false, false);

        reorderableList.drawHeaderCallback = DrawListHeader;
        reorderableList.drawFooterCallback = DrawListFooter;

        reorderableList.drawElementCallback = DrawIndividualElement;

        reorderableList.elementHeight = 25;
        reorderableList.footerHeight = 25; // for some reason footer doesn't have enough height so it's set here
    }

    private void DrawListHeader(Rect rect)
    {
        // TODO delete test code below and populate method
        Rect isValidRect = new Rect(rect.x + 10, rect.y, 55, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(isValidRect, "IsValid");                                                                                // IsValid column
        EditorGUI.LabelField(new Rect(isValidRect.xMax, rect.y, rect.max.x - 100, EditorGUIUtility.singleLineHeight), "Wave");       // Wave column
        EditorGUI.LabelField(new Rect(rect.xMax - 85, rect.y, 100, EditorGUIUtility.singleLineHeight), "WaveCount");                 // WaveCount column
    }

    private void DrawListFooter(Rect rect)
    {
        EditorGUI.LabelField(rect, "(You can reorder waves in any way you wish)");
    }

    /// <summary>
    /// Draws an element from the serialized property's list. This is called for every element of the list.
    /// </summary>    
    private void DrawIndividualElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty leveElement = levelElementsProperty.GetArrayElementAtIndex(index);
        waveProperty = leveElement.FindPropertyRelative(wavePropertyName);
        waveCountProperty = leveElement.FindPropertyRelative(waveCountPropertyName);

        #region - Validation indicator -
        bool isValidWave = EditorHelper.IsWaveValid(waveProperty.objectReferenceValue as Wave);
        Rect validationRect = new Rect(rect.x + 10, rect.y, 30, EditorGUIUtility.singleLineHeight);
        DrawValidationIndicators(validationRect, isValidWave);
        #endregion

        #region - Wave property -
        Rect wavePropertyRect = new Rect(validationRect.xMax + 10, rect.y, rect.width - 135, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(wavePropertyRect, waveProperty, GUIContent.none);
        #endregion

        #region - Wave count property -
        Rect waveCountPropertyRect = new Rect(rect.xMax - 80, rect.y, 35, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(waveCountPropertyRect, waveCountProperty, GUIContent.none);
        if (waveCountProperty.intValue < 1)
        {
            waveCountProperty.intValue = 1;
        }
        else if (waveCountProperty.intValue > 1000)
        {
            waveCountProperty.intValue = 1000; // ADD TO CONST
        }
        #endregion

        #region - Remove button -
        Rect removeButtonRect = new Rect(rect.xMax - 35, rect.y, 35, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(removeButtonRect, "-"))
        {
            // Changing this field indicates that an element needs to be removed
            // Removal cannot be done inside this method because that would cause the index to go out of bounds of the array for the last element.
            // Reorderable list would keep its original array size, which would only get updated on the next OnInspectorGUI
            // and so the removal is done after reorderableList.DoLayoutList() is finished
            indexOfElementToDelete = index;
        }
        #endregion
    }

    /// <summary>
    /// Draws buttons for adding a wave, along with other properties of the level element.
    /// </summary>
    private void DrawAddButtons()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        #region - Add new wave -
        if (GUILayout.Button(new GUIContent("Add new wave", "Copies the last wave in the list."),
            GUILayout.Height(30), GUILayout.MinWidth(130)))
        {
            ReorderableList.defaultBehaviours.DoAddButton(reorderableList);
        }
        #endregion

        #region - Add existing wave -
        if (GUILayout.Button(new GUIContent("Add existing wave", $"Shows only waves in {Constants.DataWavesPath}"),
            GUILayout.Height(30), GUILayout.MinWidth(130)))
        {
            GenericMenu menu = new GenericMenu();
            string[] guids = AssetDatabase.FindAssets("t:Wave", new string[] { Constants.DataWavesPath });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                menu.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(assetPath)), false, OnAddExistingWave, assetPath);
            }

            menu.ShowAsContext();
        }
        #endregion

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Adds a new level element with the wave property set to the wave asset located at the specified path.
    /// </summary>
    /// <param name="data">Wave asset path.</param>
    private void OnAddExistingWave(object data)
    {
        string assetPath = data as string;

        if (assetPath != null)
        {
            // Adds a new element (copies the last element) at the end
            levelElementsProperty.arraySize++;
            int indexOfLastElement = levelElementsProperty.arraySize - 1;
            SerializedProperty levelElement = levelElementsProperty.GetArrayElementAtIndex(indexOfLastElement);

            waveProperty = levelElement.FindPropertyRelative(wavePropertyName);
            waveProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Wave));

            waveCountProperty = levelElement.FindPropertyRelative(waveCountPropertyName);
            waveCountProperty.intValue = 1; // Default value 

            serializedObject.ApplyModifiedProperties();
        }
    }

    private void RemoveElementFromList()
    {
        levelElementsProperty.DeleteArrayElementAtIndex(indexOfElementToDelete);

        // Setting this back to -1 so that no unnecessairy removal is done in the next OnInspectorGUI call
        indexOfElementToDelete = -1;

        // This isn't necessary if RemoveElementFromList is being called last in the OnInspectorGUI
        serializedObject.ApplyModifiedProperties();
    }
    #endregion 

    /// <summary>
    /// Draws a validation indicator in the given rect based on the specified validity.
    /// </summary>
    /// <param name="rect">Rect where the indicators will be drawn.</param>
    /// <param name="isValid">Value for which indicator to use.</param>
    private void DrawValidationIndicators(Rect rect, bool isValid)
    {
        if (hasValidationCircles)
        {
            EditorGUI.LabelField(rect, isValid ? greenCircleContent : redCircleContent);
        }
        else
        {
            rect.x -= 10;
            rect.width += 15;
            EditorGUI.LabelField(rect, isValid ? string.Empty : "Invalid");
        }
    }

    /// <summary>
    /// Displays information about the validity of the level, i.e. if it can be used in play mode or not.
    /// </summary>
    private void DrawLevelValidityInfo()
    {
        bool isLevelValid = EditorHelper.IsLevelValid(level);
        
        Rect rect = EditorGUILayout.BeginHorizontal();

        // Validation circles
        Rect validationRect = new Rect(rect.x + 10, rect.y, 30, EditorGUIUtility.singleLineHeight);
        DrawValidationIndicators(validationRect, isLevelValid);

        // Validation label
        Rect labelRect = new Rect(validationRect.xMax + 15, rect.y, rect.width - validationRect.xMax - 15, EditorGUIUtility.singleLineHeight);
        if (isLevelValid)
        {
            EditorGUI.LabelField(labelRect, "Level can be used in play mode");
        }
        else
        {
            EditorGUI.LabelField(labelRect, new GUIContent("Level cannot be used in play mode", "Not all of its waves are valid"));
        }

        EditorGUILayout.EndHorizontal(); 
    }

    /// <summary>
    /// Draws int field with a label for the start energy property.
    /// </summary>
    private void DrawStartEnergyProperty()
    {
        GUILayout.Space(40);

        Rect rect = EditorGUILayout.BeginHorizontal();

        // Label
        Rect labelRect = new Rect(rect.x, rect.y, rect.width * 0.75f, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, "Start energy for this level:");

        // Start energy value
        Rect energyRect = new Rect(labelRect.xMax, labelRect.y, rect.width * 0.25f, EditorGUIUtility.singleLineHeight);
        startEnergyProperty.intValue = EditorGUI.IntField(energyRect, startEnergyProperty.intValue);
        startEnergyProperty.intValue = Mathf.Clamp(startEnergyProperty.intValue, 1, 10000); // ADD TO CONST 

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws int field for the start gold property.
    /// </summary>
    private void DrawStartGoldProperty()
    {
        GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.5f);

        Rect rect = EditorGUILayout.BeginHorizontal();

        // Label
        Rect labelRect = new Rect(rect.x, rect.y, rect.width * 0.75f, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, "Start gold for this level:");

        // Start gold value
        Rect startGoldRect = new Rect(labelRect.xMax, labelRect.y, rect.width * 0.25f, EditorGUIUtility.singleLineHeight);
        startGoldProperty.intValue = EditorGUI.IntField(startGoldRect, startGoldProperty.intValue);
        startGoldProperty.intValue = Mathf.Clamp(startGoldProperty.intValue, 0, 10000); // ADD TO CONST 

        EditorGUILayout.EndHorizontal();
    }
    #endregion
}
