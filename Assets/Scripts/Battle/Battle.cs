using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject currentUnitCard;
    [SerializeField] private GameObject abilitiesPanel;
    [SerializeField] private GameObject enemyUnitsPanel;
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject enemyCardPrefab;

    private static Dictionary<GameObject, Unit> _unitsPositions;
    private List<Unit> m_AllySquad;
    private List<Unit> m_EnemySquad;
    private Unit m_CurrentUnit;
    
    private GameObject[,] m_CellsGrid;
    private GameObject m_CurrentCell;
    private GameObject m_TargetCell;
    private GameObject m_ExitCell;

    private const int Width = 8;
    private const int Height = 8;

    private readonly Random m_Rnd = new Random();
    
    public void Awake()
    {
        Application.targetFrameRate = 30;
        
        var cellRect = cellPrefab.transform as RectTransform;
        cellRect.sizeDelta = new Vector2 (1.2f * 1080/Screen.height, 1.2f * 1080/Screen.height);
        var unitRect = unitPrefab.transform as RectTransform;
        unitRect.sizeDelta = new Vector2 (1.19f * 1080/Screen.height, 1.19f * 1080/Screen.height);
        _unitsPositions = new Dictionary<GameObject, Unit>();

        DrawCells();
    }

    public void Start()
    {
        m_AllySquad = SquadsManager.Squads[SquadsManager.CurrentSquad];
        
        CreateEnemySquad();
        SpawnUnitsOnField();
        UpdateEnemyUnitsPanel();
        SwitchToNextUnit();
        UpdateCurrentUnitCard();
        UpdateCells();
    }

    public void OnCellCLick()
    {
        var selectedCell = EventSystem.current.currentSelectedGameObject;
        // Если уже существует текущая клетка (подсвечивается зеленым), то назначаем targetCell
        if (m_CurrentCell != null && selectedCell != m_CurrentCell && !(_unitsPositions[m_CurrentCell].IsUsedAbility & _unitsPositions[m_CurrentCell].IsMoved))
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
            if (_unitsPositions.ContainsKey(cell))
            {
                cell.transform.Find("Unit(Clone)").Find("OnActive").gameObject.SetActive(false);
            }

            cell.transform.Find("OnTarget").gameObject.SetActive(cell == m_TargetCell);
        }
        
        m_CurrentCell.transform.Find("Unit(Clone)").Find("OnActive").gameObject.SetActive(true);

        if (m_AllySquad.Count == 0 || m_EnemySquad.Count == 0)
        {
            SceneManager.LoadScene("Map");
        }
    }

    private void UpdateCurrentUnitCard()
    {
        currentUnitCard.transform.Find("Icon").GetComponent<Image>().sprite = m_CurrentUnit.Sprite;
        currentUnitCard.transform.Find("Health").GetChild(0).GetComponent<Text>().text = m_CurrentUnit.Health.ToString();
        currentUnitCard.transform.Find("Armor").GetChild(0).GetComponent<Text>().text = m_CurrentUnit.Armor.ToString();
    }
    
    public void OnMoveButton()
    {
        var unit = m_CurrentCell.transform.Find("Unit(Clone)").gameObject;

        // Если клетка, в которую хочет переместиться игрок не содержит в себе юнита и текущий юнит ещё не ходил
        if (!_unitsPositions.ContainsKey(m_TargetCell) && m_TargetCell != m_ExitCell && !_unitsPositions[m_CurrentCell].IsMoved)
        {
            _unitsPositions[m_TargetCell] = m_CurrentUnit;
            _unitsPositions.Remove(m_CurrentCell);
            Move(unit, m_TargetCell);
            
            m_CurrentCell = m_TargetCell;
            m_TargetCell = null;
            
            _unitsPositions[m_CurrentCell].Moved();
        }
        // Если клетка, в которую хочет переместиться игрок -- клетка выхода с поля боя
        else if (m_TargetCell == m_ExitCell)
        {
            Destroy(unit);
            m_AllySquad.Remove(_unitsPositions[m_CurrentCell]);
            _unitsPositions.Remove(m_CurrentCell);

            if (m_AllySquad.Count != 0)
            {
                SwitchToNextUnit();
            }
        }
        else
        {
            Debug.Log("Ты не можешь сходить туда!");
        }

        UpdateCells();
        UpdateAvailableActions();
    }

    public void FirstAbilityButton()
    {
        if (_unitsPositions.ContainsKey(m_TargetCell) && !_unitsPositions[m_TargetCell].IsAlly && !_unitsPositions[m_CurrentCell].IsUsedAbility)
        {
            m_CurrentUnit.Ability1(_unitsPositions[m_TargetCell]);
            Debug.Log("You hit " + _unitsPositions[m_TargetCell].Name);

            if (_unitsPositions[m_TargetCell].Health <= 0)
            {
                Destroy(m_TargetCell.transform.Find("Unit(Clone)").gameObject);
                
                _unitsPositions.Remove(m_TargetCell);
            }

            m_TargetCell = null;
            
            SwitchToNextUnit();
            UpdateEnemyUnitsPanel();
            UpdateCells();
            UpdateAvailableActions();
        }
    }
    
    private void SwitchToNextUnit()
    {
        m_CurrentUnit = _unitsPositions.Values.FirstOrDefault(unit => !unit.IsUsedAbility && unit.IsAlly);
        m_CurrentCell = _unitsPositions.FirstOrDefault(x => !x.Value.IsUsedAbility & x.Value == m_CurrentUnit & x.Key != m_CurrentCell).Key;

        if (!_unitsPositions[m_CurrentCell].IsAlly)
        {
            m_CurrentUnit = null;
            m_CurrentCell = null;
        }
        m_TargetCell = null;
    }

    private void UpdateAvailableActions()
    {
        abilitiesPanel.transform.GetChild(1).GetComponent<Button>().enabled = !m_CurrentUnit.IsMoved;
        abilitiesPanel.transform.Find("ActiveAbilities").GetChild(0).GetComponent<Button>().enabled = !m_CurrentUnit.IsUsedAbility;
        
        foreach (Transform child in abilitiesPanel.transform)
        {
            child.gameObject.SetActive(m_CurrentCell != null);
        }
    }
    
    private void UpdateEnemyUnitsPanel()
    {
        foreach (Transform child in enemyUnitsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var unit in m_EnemySquad)
        {
            GameObject card = Instantiate(enemyCardPrefab, enemyUnitsPanel.transform.position, Quaternion.identity, enemyUnitsPanel.transform);
            card.transform.Find("Icon").GetComponent<Image>().sprite = unit.Sprite;
            card.transform.Find("Health").Find("Value").GetComponent<Text>().text = unit.Health.ToString();
            card.transform.Find("Armor").Find("Value").GetComponent<Text>().text = unit.Armor.ToString();
        }
    }

    private void SpawnUnitsOnField()
    {
        var exitPosition = new Point(0, 7);
        var allyPositions = GetRandomPointsArray(m_AllySquad.Count, new Point(0,3), new Point(4, 7), new List<Point>{exitPosition});
        var enemyPositions = GetRandomPointsArray(m_EnemySquad.Count, new Point(3,0), new Point(7, 4), allyPositions.Values.Append(exitPosition).ToList());
        
        m_ExitCell = m_CellsGrid[exitPosition.X, exitPosition.Y];
        m_ExitCell.transform.Find("Exit").gameObject.SetActive(true);

        for (var i=0; i<m_AllySquad.Count; i++)
        {
            var allyPrefab = unitPrefab;
            allyPrefab.transform.GetChild(0).GetComponent<Image>().sprite = m_AllySquad[i].Sprite;
            _unitsPositions.Add(m_CellsGrid[allyPositions[i].X, allyPositions[i].Y], m_AllySquad[i]);
            
            SpawnUnit(allyPrefab, allyPositions[i]);
        }
        
        for (var i=0; i<m_EnemySquad.Count; i++)
        {
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
    
    private void DrawCells()
    {
        if (cellPrefab == null)
        {
            Debug.Log("Script can not found cell prefab.");
            return;
        }

        var rectPrefab = (RectTransform) cellPrefab.transform;
        var cellSize = rectPrefab.rect.width;
        
        m_CellsGrid = new GameObject[Width, Height];
        var startPos = cellsParent.transform.position + new Vector3(cellSize/2, cellSize/2);

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                m_CellsGrid[i, j] = Instantiate(
                    cellPrefab, 
                    startPos + new Vector3(i * cellSize, j * cellSize, 0),
                    Quaternion.identity);
                m_CellsGrid[i, j].transform.SetParent(cellsParent.transform);
                m_CellsGrid[i, j].gameObject.name = i + "_" + j;
                m_CellsGrid[i, j].GetComponent<Button>().onClick.AddListener(OnCellCLick);
            }
        }
    }

    private void Move(GameObject unit, GameObject targetCell)
    {
        unit.transform.position = targetCell.transform.position;
        unit.transform.parent = targetCell.transform;
    }
    
    private void CreateEnemySquad()
    {
        m_EnemySquad = new List<Unit>
        {
            Swordsman.CreateInstance(false),
            Assault.CreateInstance(false),
            Sniper.CreateInstance(false)
        };
    }

    public void EndTurn()
    {
        foreach (var unit in _unitsPositions.Values)
        {
            unit.RefreshAbilitiesAndMoving();
        }
        
        UpdateAvailableActions();
        SwitchToNextUnit();
    }

    private Dictionary<int, Point> GetRandomPointsArray(int len, Point start, Point end, ICollection<Point> restrictedPoints)
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
