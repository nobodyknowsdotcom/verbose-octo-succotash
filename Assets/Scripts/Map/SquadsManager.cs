using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsParent;
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private GameObject squadPopup;
    [SerializeField] private Sprite[] unitIcons;
    
    private static LineRenderer _lineRenderer;
    private static int _previouslevel;
    private static int _currentSquad;
    private static Dictionary<int, int> _squadsLocation;
    private static Dictionary<int, bool> _squadsState;
    private static Dictionary<int, List<Unit>> _squads;

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
        var squad = EventSystem.current.currentSelectedGameObject;
        
        if (Int32.Parse(squad.name) == _currentSquad)
        {
            squadPopup.SetActive(true);
        }
        _currentSquad = Int32.Parse(squad.name);
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
        foreach (Transform e in squadsParent.transform)
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
        foreach (Transform squadBttn in squadsParent.transform)
        {
            squadBttn.GetComponent<Button>().onClick.AddListener(OnSquadClick);
        }
    }

    public void UpdatePopup()
    {
        var cardsParent = squadsParent.transform.Find("Panel");
    }

    private void InitSquads()
    {
        var squadsCount = squadsParent.transform.Find("Panel").childCount;
        for (int i = 0; i < squadsCount; i++)
        {
            var squad = new List<Unit>();
            squad.Append(new Unit(name = "unit0", unitIcons[0]));
            squad.Append(new Unit(name = "unit1",unitIcons[1]));
            squad.Append(new Unit(name = "unit2",unitIcons[2]));

            _squads[i] = squad;
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
