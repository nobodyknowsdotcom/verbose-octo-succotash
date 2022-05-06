using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private GameObject popup;
    private static LineRenderer _lineRenderer;
    private static Dictionary<int, List<int>> _paths;

    public void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _paths = Map.GetPaths();
    }

    public void Start()
    {
        var firstLevel = levelsParent.transform.GetChild(0).gameObject;
        UpdateLevels(levelsParent);
    }
    
    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        var currentLevelIndex = Map.GetCurrentLevel();
        // if level is available from current level 
        if (_paths[currentLevelIndex].Contains(Int32.Parse(level.name)))
        {
            SquadsManager.MoveSquad( Map.GetCurrentSquad(), Int32.Parse(level.name));
            UpdateLevels(levelsParent);
            OpenPopup();
        }
    }
    
    public static void UpdateLevels(GameObject levelsParent)
    {
        var availableLevels = Map.GetPaths();
        var drawingList = new List<GameObject>();
        var currentLevel = levelsParent.transform.GetChild(Map.GetCurrentLevel()).gameObject;
        var currentLevelIndex = Map.GetCurrentLevel();

        foreach (Transform level in levelsParent.transform)
        {
            if (level.name != currentLevelIndex.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
                level.Find("Squad_"+Map.GetCurrentSquad()).gameObject.SetActive(false);
            }
            else
            {
                level.Find("OnActive").gameObject.SetActive(true);
                level.Find("Squad_"+Map.GetCurrentSquad()).gameObject.SetActive(true);
            }
            
            if (availableLevels[currentLevelIndex].Contains(Int32.Parse(level.name)))
            {
                level.Find("OnAvailable").gameObject.SetActive(true);
                drawingList.Add(currentLevel);
                drawingList.Add(level.gameObject);
            }
            else
            {
                level.Find("OnAvailable").gameObject.SetActive(false);
            }
        }
        
        DrawLine(_lineRenderer, drawingList.ToArray());
    }

    public void OpenPopup()
    {
        popup.SetActive(true);
    }

    public static void DrawLine(LineRenderer lineRenderer, params GameObject[] objectsList)
    {
        lineRenderer.positionCount = objectsList.Length;
        for (int i = 0; i< objectsList.Length; i+=2)
        {
            var point1 = objectsList[i].transform.position;
            var point2 = objectsList[i+1].transform.position;
            var centedObject1 = new Vector3(point1.x + 0.4f,point1.y + 0.4f);
            var centedObject2 = new Vector3(point2.x + 0.4f,point2.y + 0.4f);
            lineRenderer.SetPosition(i, centedObject1);
            lineRenderer.SetPosition (i+1, centedObject2);
        }
    }
}
