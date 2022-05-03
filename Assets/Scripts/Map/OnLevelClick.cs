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
        AvailableLevels = Map.GetAvailablelevels();
        
        var firstLevel = levelsParent.transform.GetChild(0).gameObject;
        Map.SetCurrentLevel(firstLevel);
        UpdateLevels(levelsParent, Map.GetCurrentLevel());
    }
    
    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        var currentLevelIndex = Map.GetCurrentLevel();
        if (AvailableLevels[currentLevelIndex].Contains(Int32.Parse(level.name)))
        {
            Map.SetCurrentLevel(level);
        }
        
        UpdateLevels(levelsParent, Map.GetCurrentLevel());
    }

    private void UpdateLevels(GameObject parent, int currentLevel)
    {
        foreach (Transform level in levelsParent.transform)
        {
            if (level.name != currentLevel.ToString())
                level.Find("OnActive").gameObject.SetActive(false);

            if (AvailableLevels[currentLevel].Contains(Int32.Parse(level.name)))
                level.Find("OnAvailable").gameObject.SetActive(true);
            else
                level.Find("OnAvailable").gameObject.SetActive(false);
        }
    }
    
}
