using System;
using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsPanel;
    [SerializeField] private GameObject squadPopup;
    [SerializeField] private GameObject levelsParent;

    public static Dictionary<int, List<Unit>> Squads;
    
    private static LineRenderer _lineRenderer;
    private static int _previousLevel;
    public static int CurrentSquad { get; set; }
    public static Dictionary<int, bool> SquadsMovingState;
    public static Dictionary<int, bool> SquadsOrderState;
    private static Dictionary<int, int> _squadsLocation;

    public void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        
        LoadCurrentSquad();
        LoadSquadsMovingState();
        LoadSquadsOrderState();
        LoadSquadsPosition();

        InitSquadsOnMap();
        SetListeners();
        InitSquads();
    }

    public void Start()
    {
        UpdateSquadsPanel();
    }
    
    public void OnDestroy()
    {
        foreach (var squad in _squadsLocation)
        {
            PlayerPrefs.SetString("position"+squad.Key, squad.Value.ToString());
        }
        
        foreach (var squad in SquadsMovingState)
        {
            PlayerPrefs.SetString("is_moved"+squad.Key, squad.Value.ToString());
        }
        
        foreach (var squad in SquadsOrderState)
        {
            PlayerPrefs.SetString("is_ordered"+squad.Key, squad.Value.ToString());
        }
        
        PlayerPrefs.SetString("current_squad", CurrentSquad.ToString());
    }
    
    private void LoadSquadsPosition()
    {
        try
        {
            _squadsLocation = new Dictionary<int, int>
            {
                {0, Int32.Parse(PlayerPrefs.GetString("position0"))},
                {1, Int32.Parse(PlayerPrefs.GetString("position1"))}
            };
        }
        catch
        {
            _squadsLocation = new Dictionary<int, int>
            {
                {0, 0},
                {1, 0}
            };
        }
    }

    private void LoadSquadsMovingState()
    {
        try
        {
            SquadsMovingState = new Dictionary<int, bool>
            {
                {0, Boolean.Parse(PlayerPrefs.GetString("is_moved0"))},
                {1, Boolean.Parse(PlayerPrefs.GetString("is_moved1"))}
            };
        }
        catch
        {
            SquadsMovingState = new Dictionary<int, bool>
            {
                {0, false},
                {1, false}
            };
        }
    }
    
    private void LoadSquadsOrderState()
    {
        try
        {
            SquadsOrderState = new Dictionary<int, bool>
            {
                {0, Boolean.Parse(PlayerPrefs.GetString("is_ordered0"))},
                {1, Boolean.Parse(PlayerPrefs.GetString("is_ordered0"))}
            };
        }
        catch
        {
            SquadsOrderState = new Dictionary<int, bool>
            {
                {0, false},
                {1, false}
            };
        }
    }

    private void LoadCurrentSquad()
    {
        try
        {
            CurrentSquad = Int32.Parse(PlayerPrefs.GetString("current_squad"));
        }
        catch
        {
            CurrentSquad = 0;
        }
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
            Map.UpdateLevelsWithoutAvailable(levelsParent, _lineRenderer);
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
            SquadsMovingState[CurrentSquad] = false;
        }
        else
        {
            SquadsMovingState[CurrentSquad] = true;
        }
    }
    
    public static void RefreshSquadsState()
    {
        for(var i=0; i<SquadsMovingState.Count; i++)
        {
            SquadsMovingState[i] = false;
            SquadsOrderState[i] = false;
        }
    }

    private void UpdateSquadPopup(Transform popupObj, List<Unit> squad)
    {
        for (var i = 0; i < popupObj.childCount; i++)
        {
            var unitCard = popupObj.Find(i.ToString());
            
            var icon = unitCard.Find("ImageContainer").Find("Icon").GetComponent<Image>();
            icon.sprite = squad[i].Sprite;
            
            var name = unitCard.Find("NameContainer").Find("Name").GetComponent<Text>();
            name.text = squad[i].Name;
            
            var maintenancePrice = unitCard.Find("Stats").Find("Price").Find("Stat").GetComponent<Text>();
            maintenancePrice.text = squad[i].MaintenancePrice.ToString();
            
            var health = unitCard.Find("Stats").Find("Health").Find("Stat").GetComponent<Text>();
            health.text = squad[i].Health.ToString();
            
            var damage = unitCard.Find("Stats").Find("Damage").Find("Stat").GetComponent<Text>();
            damage.text = squad[i].Damage.ToString();
            
            var defense = unitCard.Find("Stats").Find("Defense").Find("Stat").GetComponent<Text>();
            defense.text = squad[i].Armor.ToString();
            
            var dodge = unitCard.Find("Stats").Find("Dodge").Find("Stat").GetComponent<Text>();
            dodge.text = squad[i].DodgeChance*100 + " %";
            
            var accuracy = unitCard.Find("Stats").Find("Accuracy").Find("Stat").GetComponent<Text>();
            accuracy.text = squad[i].Accuracy*100 + " %";
        }
    }

    private static void InitSquads()
    {
        Squads = new Dictionary<int, List<Unit>>
        {
            [0] = new List<Unit>
            {
                Swordsman.CreateInstance(true),
                Assault.CreateInstance(true),
                Sniper.CreateInstance(true)
            },
            [1] = new List<Unit>
            {
                Assault.CreateInstance(true),
                Sniper.CreateInstance(true),
                Swordsman.CreateInstance(true)
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
        foreach (Transform squadButton in squadsPanel.transform)
        {
            squadButton.GetComponent<Button>().onClick.AddListener(OnSquadClick);
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
        return SquadsMovingState;
    }
}
