using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private static Dictionary<int, int> _squadsLocation;
    private static int _currentSquad;

    private static readonly Dictionary<int, List<int>> Paths = new Dictionary<int, List<int>>
    {
        {0, new List<int>{1, 2, 3}},
        {1, new List<int>{0, 2, 4}},
        {2, new List<int>{0, 1, 4}},
        {3, new List<int>{0, 5}},
        {4, new List<int>{1, 2, 5, 6}},
        {5, new List<int>{3, 6}},
        {6, new List<int>{4, 5}},
    };

    public void Awake()
    {
        _squadsLocation = new Dictionary<int, int>()
        {
            {0, 0},
            {1, 0}
        };
        _currentSquad = 0;
    }

    public void Start()
    {
    }
    
    public void Update()
    {
    }

    public static int GetCurrentLevel()
    {
        return SquadsManager.GetSquadsLocation()[_currentSquad];
    }

    public static int GetCurrentSquad()
    {
        return _currentSquad;
    }
    
    public static void SetCurrentSquad(int index)
    {
        _currentSquad = index;
    }

    public static Dictionary<int, List<int>> GetPaths()
    {
        return Paths;
    }

    public static Dictionary<int, int> GetSquadsLocation()
    {
        return _squadsLocation;
    }
}
