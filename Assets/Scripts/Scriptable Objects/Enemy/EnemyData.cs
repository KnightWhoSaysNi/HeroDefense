using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Custom Enemy")]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;

    // ADD TO CONST
    [Range(0, 1000)] public float maxHealth;
    [Range(0, 1000)] public float currentHealth;
    [Range(0, 1000)] public float movementSpeed;
    [Range(0, 1000)] public int energyDrain;
    public float armor;
}

public class StrongEnemy
{
    [MenuItem("Assets/Create/Enemy/Strong Enemy",priority = 0)]
    [UnityEditor.MenuItem("Create/Enemy/Strong Enemy")]    
    public static void CreateStrongEnemy()
    {
        EnemyData strongEnemyData = ScriptableObject.CreateInstance<EnemyData>();
        strongEnemyData.maxHealth = 200;
        strongEnemyData.armor = 15;
        strongEnemyData.movementSpeed = 5;
        strongEnemyData.currentHealth = 200;

        AssetDatabase.CreateAsset(strongEnemyData, AssetDatabase.GenerateUniqueAssetPath("Assets/Data/Enemies/StrongEnemy.asset"));       
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = strongEnemyData;
    }
}
