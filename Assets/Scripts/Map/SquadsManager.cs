using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsParent;
    [SerializeField] private GameObject levelsParent;
    private static int _previouslevel;
    private static int _currentSquad;
    private static Dictionary<int, int> _squadsLocation;
    private static Dictionary<int, bool> _squadsState;

    public void Awake()
    {
        _currentSquad = 0;
        _squadsLocation = new Dictionary<int, int>()
        {
            {0, 0},
            {1, 0}
        };
        
        for (var i = 0; i < _squadsLocation.Count; i++)
        {
            var squadIcon = levelsParent.transform.GetChild(_squadsLocation[i]).Find("Squad_" + i);
            squadIcon.gameObject.SetActive(true);
        }
    }
    
    public void Start()
    {
        UpdateSquadsPanel(_currentSquad);
    }
    
    public void OnClick()
    {
        var squad = EventSystem.current.currentSelectedGameObject;
        _currentSquad = Int32.Parse(squad.name);
        
        UpdateSquadsPanel(_currentSquad);
        LevelManager.UpdateLevels(levelsParent);
    }
    
    
    
    private void UpdateSquadsPanel(int currentSquad)
    {
        foreach (Transform e in squadsParent.transform)
        {
            if (Int32.Parse(e.name) == currentSquad)
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

    public static void MoveSquad(int index, int levelIndex)
    {
        _previouslevel = _squadsLocation[_currentSquad];
        Debug.Log(_previouslevel);
        _squadsLocation[index] = levelIndex;
        Debug.Log(_previouslevel);
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
}
