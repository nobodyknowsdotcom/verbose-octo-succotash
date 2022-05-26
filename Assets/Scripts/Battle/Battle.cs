using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject currentUnitCard;
    [SerializeField] private GameObject abilitiesPanel;
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject unitPrefab;

    private static Dictionary<int, List<Unit>> _squads;
    private static Dictionary<GameObject, Unit> _unitsPositions;
    private List<Unit> m_EnemySquad;
    private Unit m_CurrentUnit;
    
    private GameObject[,] m_CellsGrid;
    private GameObject m_CurrentCell;
    private GameObject m_TargetCell;
    private GameObject m_ExitCell;

    private const int Width = 8;
    private const int Height = 8;
    private const float CellSize = 1.2f;

    private readonly Random m_Rnd = new Random();

    public void Awake()
    {
        _unitsPositions = new Dictionary<GameObject, Unit>();

        DrawCells();
        InitEnemySquad();
    }

    public void Start()
    {
        _squads = SquadsManager.Squads;
        m_CurrentUnit = _squads[SquadsManager.CurrentSquad][0];
        InitUnitsOnField();

        m_CurrentCell = _unitsPositions.FirstOrDefault(x => x.Value == m_CurrentUnit).Key;
        UpdateCells();
        UpdateCurrentUnitCard();
    }

    public void OnCellCLick()
    {
        var selectedCell = EventSystem.current.currentSelectedGameObject;
        // Если уже существует текущая клетка (подсвечивается зеленым), то назначаем targetCell
        if (m_CurrentCell != null && selectedCell != m_CurrentCell)
        {
            m_TargetCell = selectedCell;
        }
        else
        {
            // Если игрок выбрал текущаю клетку (произошел даблклик) -- обнуляем currentCell и targetCell 
            if (selectedCell == m_CurrentCell)
            {
                m_CurrentCell = null;
                m_TargetCell = null;
            }
            else
            {
                // Если текущая клетка ещё не назначена, то назначаем выбранную клетку текущей (если в ней находится союзный юнит)
                if (_unitsPositions.Keys.Contains(selectedCell) && _unitsPositions[selectedCell].IsAlly)
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

        m_CellsGrid = new GameObject[Width, Height];
        var startPos = cellsParent.transform.position + new Vector3(CellSize/2, CellSize/2);

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
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

        // Если клетка, в которую хочет переместиться игрок не содержит в себе юнита
        if (!_unitsPositions.ContainsKey(m_TargetCell))
        {
            _unitsPositions[m_TargetCell] = m_CurrentUnit;
            _unitsPositions.Remove(m_CurrentCell);
            Move(unit, m_TargetCell);
            
            m_CurrentCell = m_TargetCell;
            m_TargetCell = null;
            
            UpdateCells();
        }
        // Если клетка, в которую хочет переместиться игрок -- клетка выхода с поля боя
        else if (m_TargetCell == m_ExitCell)
        {
            Destroy(_unitsPositions[m_TargetCell]);
            m_CurrentUnit = null;
        }
        else
        {
            Debug.Log("Ты не можешь поставить юнита на эту клетку!");
        }
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
            Swordsman.CreateInstance("Супер монстр", false,  25, 24, 65, 12, 0.1, 0.7),
            Assault.CreateInstance("Чудовище", false, 8, 10, 35, 6, 0.02, 0.7),
            Sniper.CreateInstance("Мясо", false, 5, 8, 20, 5, 0.1, 0.7)
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
