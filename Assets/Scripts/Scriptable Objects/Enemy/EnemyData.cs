using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    // ADD TO CONST?
    [Range(0, 1000)] public float maxHealth;
    [Range(0, 100)] public float movementSpeed;
    [Range(0, 1000)] public int energyDrain;
    [Range(0, 1000)] public int armor;
    [Range(0, 1000)] public int goldReward;

    [Range(0, 1000)] public int level;
    [Range(0, 100)] public int baseExperienceReward;
}

//public class StrongEnemy
//{
//    [MenuItem("Assets/Create/Enemy/Strong Enemy", priority = 0)]
//    [UnityEditor.MenuItem("Create/Enemy/Strong Enemy")]
//    public static void CreateStrongEnemy()
//    {
//        EnemyData strongEnemyData = ScriptableObject.CreateInstance<EnemyData>();
//        strongEnemyData.maxHealth = 200;
//        strongEnemyData.armor = 15;
//        strongEnemyData.movementSpeed = 5;

//        AssetDatabase.CreateAsset(strongEnemyData, AssetDatabase.GenerateUniqueAssetPath("Assets/Data/Enemies/StrongEnemy.asset"));
//        AssetDatabase.SaveAssets();

//        EditorUtility.FocusProjectWindow();
//        Selection.activeObject = strongEnemyData;
//    }
//}
