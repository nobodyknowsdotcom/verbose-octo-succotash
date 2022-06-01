using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject turnText;
    
    private static int _turn;
    
    private static LineRenderer _lineRenderer;
    
    private static Dictionary<int, List<int>> _paths;

    public void Awake()
    {
        LoadTurn();
        _lineRenderer = GetComponent<LineRenderer>();
        
        _paths = new Dictionary<int, List<int>>
        {
            {0, new List<int>{1, 2}},
            {1, new List<int>{0, 2, 3}},
            {2, new List<int>{0, 1, 3}},
            {3, new List<int>{1, 2, 4}},
            {4, new List<int>{6}},
            {5, new List<int>{3, 4, 6}},
            {6, new List<int>{4, 5}}
        };

        UpdateTurnText();
    }

    public void Start()
    {
        UpdateLevels(levelsParent);
    }

    public void OnDestroy()
    {
        PlayerPrefs.SetString("turn", _turn.ToString());
    }

    private void LoadTurn()
    {
        try
        {
            _turn = Int32.Parse(PlayerPrefs.GetString("turn"));
        }
        catch
        {
            _turn = 1;
        }
    }

    public void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        var currentLevelIndex = GetCurrentLevel();
        var currentSquad = SquadsManager.CurrentSquad;
        // if level is available from current level 
        if (_paths[currentLevelIndex].Contains(Int32.Parse(level.name)) && !SquadsManager.GetSquadsState()[currentSquad])
        {
            SquadsManager.MoveSquad( SquadsManager.CurrentSquad, Int32.Parse(level.name), false);
            // if squad moved to another level  
            UpdateLevels(levelsParent);
        }
    }

    private static int GetCurrentLevel()
    {
        return SquadsManager.GetSquadsLocation()[SquadsManager.CurrentSquad];
    }
    
    public static void UpdateLevels(GameObject levelsParent)
    {
        var currentLevel = levelsParent.transform.GetChild(GetCurrentLevel()).gameObject;
        var currentLevelIndex = GetCurrentLevel();
        var drawingList = new List<GameObject>();

        foreach (Transform level in levelsParent.transform)
        {
            // Если данный уровень не является текущим
            if (level.name != currentLevelIndex.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
                level.Find("Squad_"+SquadsManager.CurrentSquad).gameObject.SetActive(false);
            }
            else
            {
                level.Find("OnActive").gameObject.SetActive(true);
                level.Find("Squad_"+SquadsManager.CurrentSquad).gameObject.SetActive(true);
            }
            
            // Если на данный уровень возможно перейти с текущего и текущий отряд еще не ходил
            if (_paths[currentLevelIndex].Contains(Int32.Parse(level.name)) && !SquadsManager.SquadsState[SquadsManager.CurrentSquad])
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

    public static void UpdateLevelsWithoutAvailable(GameObject parent, LineRenderer lineRenderer)
    {
        var currentLevelIndex = GetCurrentLevel();
        lineRenderer.positionCount = 0;

        foreach (Transform level in parent.transform)
        {
            // if level is not current
            if (level.name != currentLevelIndex.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
                level.Find("Squad_"+SquadsManager.CurrentSquad).gameObject.SetActive(false);
            }
            else
            {
                level.Find("OnActive").gameObject.SetActive(true);
                level.Find("Squad_"+SquadsManager.CurrentSquad).gameObject.SetActive(true);
            }
            level.Find("OnAvailable").gameObject.SetActive(false);
        }
    }

    public void OpenPopup()
    {
        popup.SetActive(true);
    }

    public void EndStep()
    {
        _turn++;
        SquadsManager.RefreshSquadsState();
        
        UpdateTurnText();
        UpdateLevels(levelsParent);
    }

    private void UpdateTurnText()
    {
        turnText.GetComponent<Text>().text = "Ход: " + _turn;
    }

    private static void DrawLine(LineRenderer lineRenderer, params GameObject[] objectsList)
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
