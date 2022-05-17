using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsPanel;
    [SerializeField] private GameObject squadPopup;
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private Sprite[] warriorIcons;
    
    private static LineRenderer _lineRenderer;
    private static int _previouslevel;
    private static int _currentSquad;
    private static Dictionary<int, int> _squadsLocation;
    private static Dictionary<int, bool> _squadsState;
    private Dictionary<int, List<Warrior>> _squads;

    public void Awake()
    {
        _currentSquad = 0;
        _lineRenderer = GetComponent<LineRenderer>();
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

    public void OnSquadClick()
    {
        var squadIcon = EventSystem.current.currentSelectedGameObject;
        
        if (Int32.Parse(squadIcon.name) == _currentSquad)
        {
            squadPopup.SetActive(true);
            UpdateSquad(squadPopup.transform.Find("SquadsParent"), _squads[_currentSquad]);
        }
        _currentSquad = Int32.Parse(squadIcon.name);
        UpdateSquadsPanel();
        
        if (GetSquadsState()[_currentSquad])
        {
            LevelManager.UpdateLevelsWithoutAviable(levelsParent, _lineRenderer);
        }
        else
        {
            LevelManager.UpdateLevels(levelsParent);
        }
    }
    
    private void UpdateSquadsPanel()
    {
        foreach (Transform e in squadsPanel.transform)
        {
            if (Int32.Parse(e.name) == _currentSquad)
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
        _previouslevel = _squadsLocation[_currentSquad];
        _squadsLocation[index] = levelIndex;
        if (isRollback)
        {
            _squadsState[_currentSquad] = false;
        }
        else
        {
            _squadsState[_currentSquad] = true;
        }
    }
    
    public static void RefreshSquadsState()
    {
        for(var i=0; i<_squadsState.Count; i++)
        {
            _squadsState[i] = false;
        }
    }

    public void UpdateSquad(Transform cardsParent, List<Warrior> squad)
    {
        for (var i = 0; i < cardsParent.childCount; i++)
        {
            var warrior = cardsParent.Find(i.ToString());
            
            var icon = warrior.Find("ImageContainer").Find("Icon").GetComponent<Image>();
            icon.sprite = squad[i].GetSprite();
            
            var name = warrior.Find("NameContainer").Find("Name").GetComponent<Text>();
            name.text = squad[i].GetName();
            
            var maintenancePrice = warrior.Find("Stats").Find("Price").Find("Stat").GetComponent<Text>();
            maintenancePrice.text = squad[i].GetMaintenance().ToString();
            
            var health = warrior.Find("Stats").Find("Health").Find("Stat").GetComponent<Text>();
            health.text = squad[i].GetHealth().ToString();
            
            var damage = warrior.Find("Stats").Find("Damage").Find("Stat").GetComponent<Text>();
            damage.text = squad[i].GetDamage().ToString();
            
            var defense = warrior.Find("Stats").Find("Defense").Find("Stat").GetComponent<Text>();
            defense.text = squad[i].GetDefennse().ToString();
            
            var dodge = warrior.Find("Stats").Find("Dodge").Find("Stat").GetComponent<Text>();
            dodge.text = squad[i].GetDodgeChance()*100 + " %";
            
            var accuracy = warrior.Find("Stats").Find("Accuracy").Find("Stat").GetComponent<Text>();
            accuracy.text = squad[i].GetAccuracy()*100 + " %";
        }
    }

    private void InitSquads()
    {
        _squads = new Dictionary<int, List<Warrior>>();
        _squads[0] = new List<Warrior>()
        {
            new Warrior("Убийца нечисти", 7, 25, 24, 70, 12, 0.1, 0.85, warriorIcons[0]),
            new Warrior("Чебупицца", 2, 8, 10, 40, 6, 0.02, 0.8, warriorIcons[1]),
            new Warrior("Бульмени", 1, 5, 8, 35, 5, 0.1, 0.7, warriorIcons[1])
        };
        
        _squads[1] = new List<Warrior>()
        {
            new Warrior("Рекрутер", 1, 2, 2, 12, 1, 0.01, 0.2, warriorIcons[0]),
            new Warrior("Ветеран", 34, 600, 160, 560, 45, 0.021, 0.92, warriorIcons[1]),
            new Warrior("Мужик", 8, 60, 47, 140, 18, 0.13, 0.85, warriorIcons[1])
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
        return _previouslevel;
    }
    
    public static int GetCurrentSquad()
    {
        return _currentSquad;
    }
    
    public static void SetCurrentSquad(int index)
    {
        _currentSquad = index;
    }

    public static Dictionary<int, bool> GetSquadsState()
    {
        return _squadsState;
    }
}
