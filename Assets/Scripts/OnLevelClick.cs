using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnLevelClick : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    private Dictionary<int, List<int>> AvailableLevels;

    public void Start()
    {
        AvailableLevels = LevelSelectionSystem.GetAvailablelevels();
    }
    
    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        var currentLevelIndex = LevelSelectionSystem.GetCurrentLevel();
        if (AvailableLevels[currentLevelIndex].Contains(Int32.Parse(level.name)))
        {
            LevelSelectionSystem.SetCurrentLevel(level);
        }
        
    }
    
}
