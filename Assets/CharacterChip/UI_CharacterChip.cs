using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public enum Character
{
    Swordsman,
    Assault,
    Sniper,
    EnemySwordsman,
    EnemyAssault,
    EnemySniper,
}

public class UI_CharacterChip : MonoBehaviour
{
    private const string enemyKeyword = "enemy";

    public Character Character { get; private set; }
    public bool IsEnemy { get; private set; }
    public float HealthFillValue { get; private set; }
    public float ArmorFillValue { get; private set; }
    public bool IsSelected { get; private set; }

    [Header("Components")]
    [SerializeField] private RectMask2D HealthFill;
    [SerializeField] private RectMask2D ArmorFill;
    [SerializeField] private GameObject EnemyHealth;
    [SerializeField] private GameObject EnemyHealthDark;
    [SerializeField] private GameObject AllyHealth;
    [SerializeField] private GameObject AllyHealthDark;
    [SerializeField] private List<GameObject> AvatarsList = new List<GameObject>();
    [SerializeField] private GameObject SelectedFill;
    [SerializeField] private GameObject SelectedBars;
    private RectTransform rectTransform;
    private readonly Dictionary<Character, GameObject> Avatars = new Dictionary<Character, GameObject>();

    public void Initialize(Character character)
    {
        Avatars.Clear();
        foreach (var a in AvatarsList)
        {
            if (!Enum.TryParse(a.name, out Character characterType))
                throw new ArgumentException($"Wrong avatar name (\"{a.name}\"). Name according to \"{nameof(Character)}\" enum.");
            Avatars.Add(characterType, a);
        }
        SetHealth(1);
        SetArmor(1);
        SetSelection(false);
        Character = character;
        IsEnemy = false;
        if (Character.ToString().ToLower().Contains(enemyKeyword))
            IsEnemy = true;
        UpdateChipStatus();
    }

    public void MoveTransformTo(Vector2 destination, float time)
        => LeanTween.move(gameObject, destination, time).setEaseInOutQuint();

    public void MoveRectTransformTo(Vector2 destination, float time)
        => LeanTween.move(GetComponent<RectTransform>(), destination, time).setEaseInOutQuint();

    public void SetSelection(bool isSelected)
    {
        IsSelected = isSelected;
        UpdateChipStatus();
    }

    public void SetHealth(float newNormalizedValue)
    {
        ValidateValueIsBetweenZeroAndOne(newNormalizedValue, out var checkedNormalized);
        HealthFillValue = checkedNormalized;
        var paddingTruncValue = 
            (1 - HealthFillValue)
            * HealthFill.rectTransform.rect.height 
            * rectTransform.localScale.y;
        LeanTween.value(
            gameObject,
            f => HealthFill.padding = new Vector4(0, 0, 0, f),
            HealthFill.padding.w,
            paddingTruncValue,
            0.3f)
            .setEaseOutCirc();
        UpdateChipStatus();
    }

    public void SetArmor(float newNormalizedValue)
    {
        ValidateValueIsBetweenZeroAndOne(newNormalizedValue, out var checkedNormalized);
        ArmorFillValue = checkedNormalized;
        var paddingTruncValue = 
            (1 - ArmorFillValue)
            * ArmorFill.rectTransform.rect.height
            * rectTransform.localScale.y;
        LeanTween.value(
            gameObject,
            f => ArmorFill.padding = new Vector4(0, 0, 0, f),
            ArmorFill.padding.w,
            paddingTruncValue,
            0.3f)
            .setEaseOutCirc();
        UpdateChipStatus();
    }


    private void ValidateValueIsBetweenZeroAndOne(float value, out float normilizedValue)
    {
        var errorValue = 0.00001f;
        if (value > 1 + errorValue || value < 0 - errorValue)
            throw new ArgumentException();
        normilizedValue = Mathf.Clamp(value, 0, 1);
    }

    private void UpdateChipStatus()
    {
        AllyHealthDark.active = EnemyHealthDark.active = AllyHealth.active = EnemyHealth.active = false;
        if (ArmorFillValue > 0)
        {
            EnemyHealthDark.SetActive(IsEnemy);
            AllyHealthDark.SetActive(!IsEnemy);
        }
        else
        {
            EnemyHealth.SetActive(IsEnemy);
            AllyHealth.SetActive(!IsEnemy);
        }
        foreach (var avatar in Avatars.Values)
            avatar.SetActive(false);
        Avatars[Character].SetActive(true);
        SelectedBars.SetActive(false);
        SelectedFill.SetActive(false);
        if (IsSelected)
        {
            SelectedBars.SetActive(true);
            SelectedFill.SetActive(true);
        }
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Initialize(Character);
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F1)) Initialize(Character.Swordsman);
    //    if (Input.GetKeyDown(KeyCode.F2)) Initialize(Character.EnemyAssault);
    //    if (Input.GetKeyDown(KeyCode.F3)) Initialize(Character.Sniper);
    //    if (Input.GetKeyDown(KeyCode.Alpha1)) SetHealth(0);
    //    if (Input.GetKeyDown(KeyCode.Alpha2)) SetHealth(0.5f);
    //    if (Input.GetKeyDown(KeyCode.Alpha3)) SetHealth(0.75f);
    //    if (Input.GetKeyDown(KeyCode.Keypad1)) SetArmor(0);
    //    if (Input.GetKeyDown(KeyCode.Keypad2)) SetArmor(0.5f);
    //    if (Input.GetKeyDown(KeyCode.Keypad3)) SetArmor(0.75f);
    //    if (Input.GetKeyDown(KeyCode.E)) MoveRectTransformTo(new Vector3(-800, 0), 0.5f);
    //    if (Input.GetKeyDown(KeyCode.Q)) SetSelection(!IsSelected);
    //}
}
