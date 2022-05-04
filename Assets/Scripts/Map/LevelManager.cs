using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    private LineRenderer lineRenderer;
    private Dictionary<int, List<int>> AvailableLevels;

    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.sortingOrder = 2;
        AvailableLevels = Map.GetAvailablelevels();
    }

    public void Start()
    {
        var firstLevel = levelsParent.transform.GetChild(0).gameObject;
        Map.SetCurrentLevel(firstLevel);
        UpdateLevels(levelsParent, Map.GetCurrentLevel());
    }
    
    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        var currentLevelIndex = Map.GetCurrentLevel();
        
        // Переходим на указанный уровень, если он доступен
        if (AvailableLevels[currentLevelIndex].Contains(Int32.Parse(level.name)))
        {
            Map.SetCurrentLevel(level);
        }

        UpdateLevels(levelsParent, Map.GetCurrentLevel());
    }

    private void UpdateLevels(GameObject parent, int currentLevel)
    {
        var drawingList = new List<GameObject>();
        var currentLevelObj = levelsParent.transform.GetChild(Map.GetCurrentLevel()).gameObject;
        foreach (Transform level in parent.transform)
        {
            
            if (level.name != currentLevel.ToString())
                level.Find("OnActive").gameObject.SetActive(false);

            if (AvailableLevels[currentLevel].Contains(Int32.Parse(level.name)))
            {
                level.Find("OnAvailable").gameObject.SetActive(true);
                drawingList.Add(currentLevelObj);
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
