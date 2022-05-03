using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
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
    }
    
    public void Update()
    {
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

    public static int GetCurrentSquad()
    {
        return _currentSquad;
    }
    
    public static void SetCurrentSquad(GameObject squad)
    {
        _currentSquad = Int32.Parse(squad.name);
    }

    public static Dictionary<int, List<int>> GetAvailablelevels()
    {
        return AvailableLevels;
    }
}
