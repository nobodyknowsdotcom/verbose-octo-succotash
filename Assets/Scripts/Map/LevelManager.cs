using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    private LineRenderer lineRenderer;
    private Dictionary<int, List<int>> Paths;

    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.sortingOrder = 2;
        Paths = Map.GetPaths();
    }

    public void Start()
    {
        var firstLevel = levelsParent.transform.GetChild(0).gameObject;
        Map.SetCurrentLevel(firstLevel);
        UpdateLevels(Map.GetCurrentLevel());
    }
    
    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        var currentLevelIndex = Map.GetCurrentLevel();
        
        if (Paths[currentLevelIndex].Contains(Int32.Parse(level.name)))
        {
            Map.SetCurrentLevel(level);
            SquadsManager.MoveSquad( Map.GetCurrentSquad(), Int32.Parse(level.name));
        }
        UpdateLevels(Map.GetCurrentLevel());
    }
    
    private void UpdateLevels(int currentLevel)
    {
        var drawingList = new List<GameObject>();
        var currentLevelAsObject = levelsParent.transform.GetChild(Map.GetCurrentLevel()).gameObject;

        foreach (Transform level in levelsParent.transform)
        {
            if (level.name != currentLevel.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
                level.Find("Squad_"+Map.GetCurrentSquad()).gameObject.SetActive(false);
            }
            
            if (Paths[currentLevel].Contains(Int32.Parse(level.name)))
            {
                level.Find("OnAvailable").gameObject.SetActive(true);
                drawingList.Add(currentLevelAsObject);
                drawingList.Add(level.gameObject);
            }
            else
            {
                level.Find("OnAvailable").gameObject.SetActive(false);
            }
        }
        
        DrawLine(drawingList.ToArray());
    }
    
    private void DrawLine(params GameObject[] objectsList)
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
