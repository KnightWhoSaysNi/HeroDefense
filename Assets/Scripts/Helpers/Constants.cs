using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // Tags
    public const string EnemyTag = "Enemy";
    public const string PlaceableTag = "Placeable";
    public const string SellableTag = "Sellable";

    // Scene names
    public const string MainMenuSceneName = "MainMenu";
    public const string GameplayUISceneName = "GameplayUI";
    public const string TutorialSceneName = "TutorialLevel";

    // UI messages
    public const string LevelStartMessage = "Press G to start";
    public const string WaveStartedMessage = "Wave started";
    public const string FinalWaveMessage = "Final wave";

    // Trap animator parameters
    public const string TrapAnimatorAttackedTrigger = "Attacked";
    public const string TrapAnimatorIsAttackingBool = "IsAttacking";

    // Asset paths
    public const string TexturesGreenCirclePath = "Assets/Textures/Editor/GreenCircle.png";
    public const string TexturesRedCirclePath = "Assets/Textures/Editor/RedCircle.png";
    public const string DataWavesPath = "Assets/Data/Waves";
}
