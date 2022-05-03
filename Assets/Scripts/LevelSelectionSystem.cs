using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelSelectionSystem : MonoBehaviour
{
    
    [SerializeField] public GameObject levelsParent;
    private static int _currentLevel;
    private static int _currentSquad;

    private static readonly Dictionary<int, List<int>> AvailableLevels = new Dictionary<int, List<int>>
    {
        {0, new List<int>{1, 2, 3}},
        {1, new List<int>{0, 2, 4}},
        {2, new List<int>{0, 1, 4}},
        {3, new List<int>{0, 5}},
        {4, new List<int>{1, 2, 5, 6}},
        {5, new List<int>{3, 6}},
        {6, new List<int>{4, 5}},
    };
    
    public void Start()
    {
        var firstLevel = levelsParent.transform.GetChild(0).gameObject;
        SetCurrentLevel(firstLevel);
    }
    
    public void Update()
    {
        foreach (Transform level in levelsParent.transform)
        {
            if (level.name != _currentLevel.ToString())
                level.Find("OnActive").gameObject.SetActive(false);

            if (AvailableLevels[_currentLevel].Contains(Int32.Parse(level.name)))
                level.Find("OnAvailable").gameObject.SetActive(true);
            else
                level.Find("OnAvailable").gameObject.SetActive(false);
        }
    }

    public static void SetCurrentLevel(GameObject level)
    {
        var enabledIcon = level.transform.Find("OnActive").gameObject;
        enabledIcon.SetActive(true);
        _currentLevel = Int32.Parse(level.name);
    }

    public static int GetCurrentLevel()
    {
        return _currentLevel;
    }

    public int GetCurrentSquad()
    {
        return _currentSquad;
    }
    
    public void SetCurrentSquad(int squad)
    {
        _currentSquad =  squad;
    }

    public static Dictionary<int, List<int>> GetAvailablelevels()
    {
        return AvailableLevels;
    }
}
