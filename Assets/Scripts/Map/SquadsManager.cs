using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsParent;
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private GameObject squadPopup;
    
    private static LineRenderer _lineRenderer;
    private static int _previouslevel;
    private static int _currentSquad;
    private static Dictionary<int, int> _squadsLocation;
    private static Dictionary<int, bool> _squadsState;

    public void Awake()
    {
        _currentSquad = 0;
        _lineRenderer = GetComponent<LineRenderer>();
        _squadsLocation = new Dictionary<int, int>()
        {
            {0, 0},
            {1, 0}
        };
        _squadsState = new Dictionary<int, bool>()
        {
            {0, false},
            {1, false}
        };
        
        for (var i = 0; i < _squadsLocation.Count; i++)
        {
            var squadIcon = levelsParent.transform.GetChild(_squadsLocation[i]).Find("Squad_" + i);
            squadIcon.gameObject.SetActive(true);
        }
    }
    
    public void Start()
    {
        UpdateSquadsPanel();
    }
    
    public void OnClick()
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

    public static Dictionary<int, int> GetSquadsLocation()
    {
        return _squadsLocation;
    }

    public static void SetSquadsLocation(Dictionary<int, int> location)
    {
        _squadsLocation = location;
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
