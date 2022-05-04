using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsParent;
    [SerializeField] private GameObject levelsParent;
    private LineRenderer lineRenderer;
    private static Dictionary<int, int> _squadsLocation;

    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        _squadsLocation = Map.GetSquadsLocation();
        
        for (var i = 0; i < _squadsLocation.Count; i++)
        {
            var squadIcon = levelsParent.transform.GetChild(_squadsLocation[i]).Find("Squad_" + i);
            squadIcon.gameObject.SetActive(true);
        }
    }
    
    public void Start()
    {
        UpdateSquadsPanel(Map.GetCurrentSquad());
    }
    
    public void OnClick()
    {
        var squad = EventSystem.current.currentSelectedGameObject;
        Map.SetCurrentSquad(Int32.Parse(squad.name));
        
        UpdateSquadsPanel(Map.GetCurrentSquad());
        UpdateLevels(Map.GetCurrentLevel());
    }
    
    private void UpdateLevels(int currentLevel)
    {
        var availableLevels = Map.GetPaths();
        var drawingList = new List<GameObject>();
        var currentLevelAsObject = levelsParent.transform.GetChild(Map.GetCurrentLevel()).gameObject;

        foreach (Transform level in levelsParent.transform)
        {
            if (level.name != currentLevel.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
                level.Find("Squad_"+Map.GetCurrentSquad()).gameObject.SetActive(false);
            }
            else
            {
                level.Find("OnActive").gameObject.SetActive(true);
                level.Find("Squad_"+Map.GetCurrentSquad()).gameObject.SetActive(true);
            }
            
            if (availableLevels[currentLevel].Contains(Int32.Parse(level.name)))
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
        _squadsLocation[index] = levelIndex;
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
