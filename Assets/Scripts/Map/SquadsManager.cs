using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsPanel;
    [SerializeField] private GameObject squadPopup;
    [SerializeField] private GameObject levelsParent;
    // [allyAssault, allyKnight, allySniper, enemyAssault, enemy...]
    public Sprite[] warriorIcons;
    
    public static Sprite[] StaticWarriorIcons;
    public static Dictionary<int, List<Warrior>> Squads;
    
    private static LineRenderer _lineRenderer;
    private static int _previousLevel;
    public static int CurrentSquad { get; set; }
    private static Dictionary<int, int> _squadsLocation;
    private static Dictionary<int, bool> _squadsState;

    public void Awake()
    {
        CurrentSquad = 0;
        _lineRenderer = GetComponent<LineRenderer>();
        StaticWarriorIcons = warriorIcons;
        _squadsLocation = new Dictionary<int, int>
        {
            {0, 0},
            {1, 0}
        };
        _squadsState = new Dictionary<int, bool>
        {
            {0, false},
            {1, false}
        };
        
        InitSquadsOnMap();
        SetListeners();
        InitSquads();
    }

    public void Start()
    {
        UpdateSquadsPanel();
    }

    private void OnSquadClick()
    {
        var squadIcon = EventSystem.current.currentSelectedGameObject;
        
        if (Int32.Parse(squadIcon.name) == CurrentSquad)
        {
            squadPopup.SetActive(true);
            UpdateSquadPopup(squadPopup.transform.Find("SquadsParent"), Squads[CurrentSquad]);
        }
        CurrentSquad = Int32.Parse(squadIcon.name);
        UpdateSquadsPanel();
        
        if (GetSquadsState()[CurrentSquad])
        {
            Map.UpdateLevelsWithoutAviable(levelsParent, _lineRenderer);
        }
        else
        {
            Map.UpdateLevels(levelsParent);
        }
    }
    
    private void UpdateSquadsPanel()
    {
        foreach (Transform e in squadsPanel.transform)
        {
            if (Int32.Parse(e.name) == CurrentSquad)
            {
                var enabledIcon = e.transform.Find("OnActive").gameObject;
                enabledIcon.SetActive(true);
            }
            else
            {
                var enabledIcon = e.transform.Find("OnActive").gameObject;
                enabledIcon.SetActive(false);
            }
        }
    }
    
    public static void MoveSquad(int index, int levelIndex, bool isRollback)
    {
        _previousLevel = _squadsLocation[CurrentSquad];
        _squadsLocation[index] = levelIndex;
        if (isRollback)
        {
            _squadsState[CurrentSquad] = false;
        }
        else
        {
            _squadsState[CurrentSquad] = true;
        }
    }
    
    public static void RefreshSquadsState()
    {
        for(var i=0; i<_squadsState.Count; i++)
        {
            _squadsState[i] = false;
        }
    }

    private void UpdateSquadPopup(Transform popupObj, List<Warrior> squad)
    {
        for (var i = 0; i < popupObj.childCount; i++)
        {
            var warrior = popupObj.Find(i.ToString());
            
            var icon = warrior.Find("ImageContainer").Find("Icon").GetComponent<Image>();
            icon.sprite = squad[i].Sprite;
            
            var name = warrior.Find("NameContainer").Find("Name").GetComponent<Text>();
            name.text = squad[i].Name;
            
            var maintenancePrice = warrior.Find("Stats").Find("Price").Find("Stat").GetComponent<Text>();
            maintenancePrice.text = squad[i].MaintenancePrice.ToString();
            
            var health = warrior.Find("Stats").Find("Health").Find("Stat").GetComponent<Text>();
            health.text = squad[i].Health.ToString();
            
            var damage = warrior.Find("Stats").Find("Damage").Find("Stat").GetComponent<Text>();
            damage.text = squad[i].Damage.ToString();
            
            var defense = warrior.Find("Stats").Find("Defense").Find("Stat").GetComponent<Text>();
            defense.text = squad[i].Armor.ToString();
            
            var dodge = warrior.Find("Stats").Find("Dodge").Find("Stat").GetComponent<Text>();
            dodge.text = squad[i].DodgeChance*100 + " %";
            
            var accuracy = warrior.Find("Stats").Find("Accuracy").Find("Stat").GetComponent<Text>();
            accuracy.text = squad[i].Accuracy*100 + " %";
        }
    }

    private static void InitSquads()
    {
        Squads = new Dictionary<int, List<Warrior>>
        {
            [0] = new List<Warrior>
            {
                Warrior.CreateInstance("Убийца нечисти",  25, 24, 70, 12, 0.1, 0.85, StaticWarriorIcons[0]),
                Warrior.CreateInstance("Чебупицца", 8, 10, 40, 6, 0.02, 0.8, StaticWarriorIcons[1]),
                Warrior.CreateInstance("Бульмени", 5, 8, 35, 5, 0.1, 0.7, StaticWarriorIcons[2])
            },
            [1] = new List<Warrior>
            {
                Warrior.CreateInstance("Рекрутер", 2, 2, 12, 1, 0.01, 0.2, StaticWarriorIcons[2]),
                Warrior.CreateInstance("Ветеран", 600, 160, 560, 45, 0.021, 0.92, StaticWarriorIcons[2]),
                Warrior.CreateInstance("Мужик", 60, 47, 140, 18, 0.13, 0.85, StaticWarriorIcons[1])
            }
        };
    }
    
    private void InitSquadsOnMap()
    {
        for (var i = 0; i < _squadsLocation.Count; i++)
        {
            var squadIcon = levelsParent.transform.GetChild(_squadsLocation[i]).Find("Squad_" + i);
            squadIcon.gameObject.SetActive(true);
        }
    }
    
    private void SetListeners()
    {
        foreach (Transform squadBttn in squadsPanel.transform)
        {
            squadBttn.GetComponent<Button>().onClick.AddListener(OnSquadClick);
        }
    }

    public static Dictionary<int, int> GetSquadsLocation()
    {
        return _squadsLocation;
    }

    public static void SetSquadsLocation(Dictionary<int, int> location)
    {
        _squadsLocation = location;
    }

    public static int GetSquadsLocationBuffer()
    {
        return _previousLevel;
    }

    public static Dictionary<int, bool> GetSquadsState()
    {
        return _squadsState;
    }
}
