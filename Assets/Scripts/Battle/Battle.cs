using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject currentUnitCard;
    [SerializeField] private GameObject abilitiesPanel;

    private static Dictionary<int, List<Unit>> _squads;
    private static Dictionary<GameObject, Unit> _unitsPositions;
    private List<Unit> m_EnemySquad;
    private Unit m_CurrentUnit;
    
    private GameObject[,] m_CellsGrid;
    private GameObject m_CurrentCell;
    private GameObject m_TargetCell;
    private GameObject m_ExitCell;
    private int m_Width;
    private int m_Height;
    private const float CellSize = 1.2f;

    private readonly Random m_Rnd = new Random();

    public void Awake()
    {
        _unitsPositions = new Dictionary<GameObject, Unit>();
        m_Width = 8;
        m_Height = 8;
        
        DrawCells();
        InitEnemySquad();
    }

    public void Start()
    {
        _squads = SquadsManager.Squads;
        m_CurrentUnit = _squads[SquadsManager.CurrentSquad][0];
        InitUnitsOnField();
    }

    public void OnCellCLick()
    {
        var selectedCell = EventSystem.current.currentSelectedGameObject;
        // set active targetCell if currentCell exists and not doubleclicked
        if (m_CurrentCell != null && selectedCell != m_CurrentCell)
        {
            m_TargetCell = selectedCell;
        }
        else
        {
            // if currentCell doubleclicked, it will reset current cell and target cells
            if (selectedCell == m_CurrentCell)
            {
                m_CurrentCell = null;
                m_TargetCell = null;
            }
            else
            {
                if (_unitsPositions.Keys.Contains(selectedCell))
                {
                    m_CurrentUnit = _unitsPositions[selectedCell];
                    m_CurrentCell = selectedCell;
                }
            }
        }

        UpdateCells();
        UpdateCurrentUnitCard();
    }
    
    private void UpdateCells()
    {
        foreach (var cell in m_CellsGrid)
        {
            cell.transform.Find("OnActive").gameObject.SetActive(cell == m_CurrentCell);
            cell.transform.Find("OnTarget").gameObject.SetActive(cell == m_TargetCell);
        }
    }
    
    private void UpdateCurrentUnitCard()
    {
        currentUnitCard.transform.Find("Icon").GetComponent<Image>().sprite = m_CurrentUnit.Sprite;
        currentUnitCard.transform.Find("Health").GetChild(0).GetComponent<Text>().text = m_CurrentUnit.Health.ToString();
        currentUnitCard.transform.Find("Armor").GetChild(0).GetComponent<Text>().text = m_CurrentUnit.Armor.ToString();
    }
    
    private void DrawCells()
    {
        if (cellPrefab == null)
        {
            Debug.Log("Script can not found cell prefab.");
            return;
        }

        m_CellsGrid = new GameObject[m_Width, m_Height];
        var startPos = cellsParent.transform.position + new Vector3(CellSize/2, CellSize/2);

        for (int i = 0; i < m_Height; i++)
        {
            for (int j = 0; j < m_Width; j++)
            {
                m_CellsGrid[i, j] = Instantiate(
                    cellPrefab, 
                    startPos + new Vector3(i * CellSize, j * CellSize, 0),
                    Quaternion.identity);
                m_CellsGrid[i, j].transform.SetParent(cellsParent.transform);
                m_CellsGrid[i, j].gameObject.name = i + "_" + j;
                m_CellsGrid[i, j].GetComponent<Button>().onClick.AddListener(OnCellCLick);
            }
        }
    }

    private void InitUnitsOnField()
    {
        var exitPosition = new Point(0, 7);
        var allyPositions = GetRandomPointsArray(3, new Point(0,3), new Point(4, 7), new List<Point>{exitPosition});
        var enemyPositions = GetRandomPointsArray(3, new Point(3,0), new Point(7, 4), allyPositions.Values.Append(exitPosition).ToList());

        m_ExitCell = m_CellsGrid[exitPosition.X, exitPosition.Y].transform.Find("Exit").gameObject;
        m_ExitCell.SetActive(true);

        for (var i=0; i<_squads[SquadsManager.CurrentSquad].Count; i++)
        {
            var allyPrefab = unitPrefab;
            allyPrefab.transform.GetChild(0).GetComponent<Image>().sprite = _squads[SquadsManager.CurrentSquad][i].Sprite;
            _unitsPositions.Add(m_CellsGrid[allyPositions[i].X, allyPositions[i].Y], _squads[SquadsManager.CurrentSquad][i]);
            SpawnUnit(allyPrefab, allyPositions[i]);
            
            var enemyPrefab = unitPrefab;
            enemyPrefab.transform.GetChild(0).GetComponent<Image>().sprite = m_EnemySquad[i].Sprite;
            _unitsPositions.Add(m_CellsGrid[enemyPositions[i].X, enemyPositions[i].Y], m_EnemySquad[i]);
            SpawnUnit(enemyPrefab, enemyPositions[i]);
        }
    }

    private void SpawnUnit(GameObject prefab, Point position)
    {
        var unit = Instantiate(prefab,
            m_CellsGrid[position.X, position.Y].transform.position, Quaternion.identity);
        unit.transform.SetParent(m_CellsGrid[position.X, position.Y].transform);
    }
    
    public void OnMoveButton()
    {
        var unit = m_CurrentCell.transform.Find("Unit(Clone)").gameObject;
        
        _unitsPositions[m_TargetCell] = m_CurrentUnit;
        _unitsPositions.Remove(m_CurrentCell);
        Move(unit, m_TargetCell);

        m_CurrentCell = m_TargetCell;
        m_TargetCell = null;
        
        UpdateCells();
    }
    
    private void Move(GameObject unit, GameObject targetCell)
    {
        unit.transform.position = targetCell.transform.position;
        unit.transform.parent = targetCell.transform;
    }
    
    private void InitEnemySquad()
    {
        m_EnemySquad = new List<Unit>
        {
            Unit.CreateInstance("Супер монстр",  25, 24, 65, 12, 0.1, 0.7, SquadsManager.StaticUnitsIcons[3]),
            Unit.CreateInstance("Чудовище", 8, 10, 35, 6, 0.02, 0.7, SquadsManager.StaticUnitsIcons[4]),
            Unit.CreateInstance("Мясо",  5, 8, 20, 5, 0.1, 0.7, SquadsManager.StaticUnitsIcons[5])
        };
    }
    
    private Dictionary<int, Point> GetRandomPointsArray(int len, Point start, Point end, List<Point> restrictedPoints)
    {
        var result = new Dictionary<int, Point>();

        for (var i = 0; i < len; i++)
        {
            var x = m_Rnd.Next(end.X - start.X);
            var y = m_Rnd.Next(end.Y - start.Y);
            var point = new Point(x + start.X, y + start.Y);

            if (!result.ContainsValue(point) && !restrictedPoints.Contains(point))
                result[i] = point;
            else
                i--;
        }

        return result;
    }

}
