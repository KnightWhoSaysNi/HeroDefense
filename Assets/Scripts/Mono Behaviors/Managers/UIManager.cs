using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Player Stats")]
    public Text playerLevel;
    public Text playerExperience;
    public Slider playerExperienceBar;
    [Space(5)]
    public Text playerGold;

    [Header("Level Information")]
    public Text wave;
    public Text energy;

    [Header("Trap Thumbnails")]
    public Sprite spikes;
    public Sprite firePillar;
    public Sprite magicCrystal;

    [Header("Slots")]
    public GameObject slotsCanvas;
    public UISlot[] slots;

    [Header("Enemy Health")]
    public GameObject enemyHealthCanvas;
    public Text enemyHealth;
    public Text enemyArmor;
    public Slider enemyHealthBar;

    private void Awake()
    {
        // Instead of making this class a "singleton" a simple check is made. This way only GameManager game object can have this mono behavior attached
        //if (GetComponent<GameManager>() == null)
        //{
        //    throw new UnityException($"{gameObject.name} game object has a {typeof(UIManager)} MonoBehavior attached. Only GameManager is allowed to have that script.");
        //}
    }

    private void Start()
    {

    }

    private void Update()
    {
    }
}

[System.Serializable]
public struct UISlot
{
    public Image image;
    public Text goldCost;
    public GameObject selectionHighlight;
}
