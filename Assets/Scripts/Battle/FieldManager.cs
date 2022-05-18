using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FieldManager : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject cellsField;
    [SerializeField] private GameObject allyWarriorPrefab;
    [SerializeField] private GameObject enemyWarriorPrefab;
    private GameObject[,] _gameGrid;
    private string _currentCell;
    private string _targetCell;
    private int _width;
    private int _height;
    private float cellSize = 1.2f;

    public void Awake()
    {
        _width = 8;
        _height = 8;
    }

    public void Start()
    {
        DrawCells();
        InitWarriorsOnField();
    }

    private void DrawCells()
    {
        if (cellPrefab == null)
        {
            Debug.Log("Script can not found cell prefab.");
            return;
        }

        _gameGrid = new GameObject[_width, _height];
        var startPos = cellsField.transform.position + new Vector3(cellSize/2, cellSize/2);

        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                _gameGrid[i, j] = Instantiate(
                    cellPrefab, 
                    startPos + new Vector3(i * cellSize, j * cellSize, 0),
                    Quaternion.identity);
                _gameGrid[i, j].transform.SetParent(cellsField.transform);
                _gameGrid[i, j].gameObject.name = i + "_" + j;
                _gameGrid[i, j].GetComponent<Button>().onClick.AddListener(OnCellCLick);
            }
        }
    }

    public void OnCellCLick()
    {
        var cell = EventSystem.current.currentSelectedGameObject;
        // set active targetCell if currentCell exists and not doubleclicked
        if (_currentCell != null && _currentCell != cell.name)
        {
            _targetCell = cell.name;
        }
        else
        {
            // if currentCell doubleclicked, it will reset current and target cells
            if (_currentCell == cell.name)
            {
                _currentCell = null;
                _targetCell = null;
            }
            else
            {
                _currentCell = cell.name;
            }
        }

        UpdateGrid();
    }

    private void UpdateGrid()
    {
        foreach (var cell in _gameGrid)
        {
           cell.transform.Find("OnActive").gameObject.SetActive(cell.name == _currentCell);
           cell.transform.Find("OnTarget").gameObject.SetActive(cell.name == _targetCell);
        }
    }

    private void InitWarriorsOnField()
    {
        var positions = RndArray(6);

        foreach (var pos in positions.Take(3))
        {
            var warrior = Instantiate(allyWarriorPrefab, _gameGrid[pos[0], pos[1]].transform.position, Quaternion.identity);
            warrior.transform.SetParent(_gameGrid[pos[0], pos[1]].transform);
        }
        
        foreach (var pos in positions.Skip(3).Take(3))
        {
            var warrior = Instantiate(enemyWarriorPrefab, _gameGrid[pos[0], pos[1]].transform.position, Quaternion.identity);
            warrior.transform.SetParent(_gameGrid[pos[0], pos[1]].transform);
        }
    }
    
    private static int[][] RndArray(int len)
    {
        var result = new int[len][];
        var rnd = new System.Random();

        for (int i = 0; i < len; i++)
        {
            var x = rnd.Next(8);
            var y = rnd.Next(8);
            var point = new []{ x, y };

            if (!Contain(result, point)) result[i] = point;
            else i--;
        }

        return result;
    }

    private static bool Contain(int[][] array, int[] search)
    {
        foreach (var e in array)
            if (Equal(e, search)) return Equal(e, search);

        return false;
    }

    private static bool Equal(int[] a1, int[] a2)
    {
        if (a1 is null || a1.Length != a2.Length) return false;

        for (int i = 0; i < a1.Length; i++)
            if (a1[i] != a2[i]) return false;

        return true;
    }
}
