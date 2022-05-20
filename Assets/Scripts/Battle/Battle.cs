using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject cellsField;
    [SerializeField] private GameObject wariorPrefab;
    [SerializeField] private GameObject currentWarriorCard;

    private static Dictionary<int, List<Warrior>> _squads;
    private List<Warrior> _enemySquad;
    private static Dictionary<GameObject, Warrior> _squadsPositions;
    private GameObject[,] _cellsGrid;
    private string _currentCell;
    private string _targetCell;
    private int _width;
    private int _height;
    private float cellSize = 1.2f;
    
    static Random rnd = new Random();

    public void Awake()
    {
        _squadsPositions = new Dictionary<GameObject, Warrior>();
        _width = 8;
        _height = 8;
    }

    public void Start()
    {
        DrawCells();
        InitEnemySquad();
        _squads = SquadsManager.Squads;
        InitWarriorsOnField();
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
    
    private void DrawCells()
    {
        if (cellPrefab == null)
        {
            Debug.Log("Script can not found cell prefab.");
            return;
        }

        _cellsGrid = new GameObject[_width, _height];
        var startPos = cellsField.transform.position + new Vector3(cellSize/2, cellSize/2);

        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                _cellsGrid[i, j] = Instantiate(
                    cellPrefab, 
                    startPos + new Vector3(i * cellSize, j * cellSize, 0),
                    Quaternion.identity);
                _cellsGrid[i, j].transform.SetParent(cellsField.transform);
                _cellsGrid[i, j].gameObject.name = i + "_" + j;
                _cellsGrid[i, j].GetComponent<Button>().onClick.AddListener(OnCellCLick);
            }
        }
    }

    private void UpdateGrid()
    {
        foreach (var cell in _cellsGrid)
        {
           cell.transform.Find("OnActive").gameObject.SetActive(cell.name == _currentCell);
           cell.transform.Find("OnTarget").gameObject.SetActive(cell.name == _targetCell);
        }
    }

    private void UpdateSelectedWarriorCard()
    {
        
    }

    private void InitWarriorsOnField()
    {
        var allyPositions = RndArray(3, new Point(0,0), new Point(4, 8), new List<Point>());
        var enemyPositions = RndArray(3, new Point(4,0), new Point(8, 8), new List<Point>());

        for (int i=0; i<_squads[SquadsManager.CurrentSquad].Count; i++)
        {
            var allyPrefab = wariorPrefab;
            allyPrefab.transform.GetChild(0).GetComponent<Image>().sprite =
                _squads[SquadsManager.CurrentSquad][i].Sprite;
            _squadsPositions.Add(_cellsGrid[allyPositions[i].X, allyPositions[i].Y], _squads[SquadsManager.CurrentSquad][i]);
            SpawnWarrior(allyPrefab, i, allyPositions);
            
            var enemyPrefab = wariorPrefab;
            enemyPrefab.transform.GetChild(0).GetComponent<Image>().sprite =
                _enemySquad[i].Sprite;
            _squadsPositions.Add(_cellsGrid[enemyPositions[i].X, enemyPositions[i].Y], _enemySquad[i]);
            SpawnWarrior(enemyPrefab, i, enemyPositions);
        }
        
        RefreshStamia(_enemySquad);
        RefreshStamia(_squads[SquadsManager.CurrentSquad]);
    }

    private void SpawnWarrior(GameObject prefab, int positionIndex, Dictionary<int, Point> positions)
    {
        var warrior = Instantiate(prefab,
            _cellsGrid[positions[positionIndex].X, positions[positionIndex].Y].transform.position, Quaternion.identity);
        warrior.transform.SetParent(_cellsGrid[positions[positionIndex].X, positions[positionIndex].Y].transform);
    }
    
    private void InitEnemySquad()
    {
        _enemySquad = new List<Warrior>
        {
            new Warrior("Супер монстр", 7, 25, 24, 65, 12, 0.1, 0.7, SquadsManager.StaticWarriorIcons[3]),
            new Warrior("Чудовище", 2, 8, 10, 35, 6, 0.02, 0.7, SquadsManager.StaticWarriorIcons[4]),
            new Warrior("Мясо", 1, 5, 8, 20, 5, 0.1, 0.7, SquadsManager.StaticWarriorIcons[5])
        };
    }
    
    private static Dictionary<int, Point> RndArray(int len, Point start, Point end, List<Point> restrictedPoints)
    {
        var result = new Dictionary<int, Point>();

        for (int i = 0; i < len; i++)
        {
            var x = rnd.Next(end.X - start.X);
            var y = rnd.Next(end.Y - start.Y);
            var point = new Point(x, y);

            if (!result.ContainsValue(point) && !restrictedPoints.Contains(point))
                result[i] = new Point(point.X + start.X, point.Y + start.Y);
            else 
                i--;
        }

        return result;
    }

    private void RefreshStamia(List<Warrior> squad)
    {
        foreach (var warrior in squad)
        {
            warrior.Stamia = 100;
        }
    }

}
