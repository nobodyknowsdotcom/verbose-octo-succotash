using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class TestBattle : MonoBehaviour
{
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject currentUnitCard;
    [SerializeField] private GameObject abilitiesPanel;
    [SerializeField] private GameObject enemyUnitsPanel;
    [SerializeField] private GameObject endTurnButton;

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject enemyCardPrefab;
    
    [SerializeField] private GameObject allyInfantrymanPrefab;
    [SerializeField] private GameObject allySniperPrefab;
    [SerializeField] private GameObject allySwordsmanPrefab;
    [SerializeField] private GameObject enemyInfantrymanPrefab;
    [SerializeField] private GameObject enemySniperPrefab;
    [SerializeField] private GameObject enemySwordsmanPrefab;

    private static Dictionary<GameObject, BattleUnit> _unitsPositions;
    private List<BattleUnit> m_AllySquad;
    private List<BattleUnit> m_EnemySquad;
    private BattleUnit m_CurrentUnit;

    private GameObject[,] m_CellsGrid;
    private List<GameObject> m_AvailableCells;
    private List<GameObject> m_ReachableCells;
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
        cellRect.sizeDelta = new Vector2(1.218f * 1080 / Screen.height, 1.218f * 1080 / Screen.height);
        _unitsPositions = new Dictionary<GameObject, BattleUnit>();

        // Над этим подумать
        // Заглушка - ниже в коде эти значения меняются
        m_AllySquad = new List<BattleUnit>
        {
            allyInfantrymanPrefab.GetComponent<BattleUnit>(),
            allySwordsmanPrefab.GetComponent<BattleUnit>(),
            allySniperPrefab.GetComponent<BattleUnit>()
        };

        DrawCells();
    }

    public void Start()
    {
        CreateEnemySquad();
        SpawnUnitsOnField();
        SwitchToNextUnit();

        m_AvailableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.abilities.primaryMoveSkill.range);
        m_ReachableCells = new List<GameObject>();
    }

    public void Update()
    {
        UpdateCells();

        UpdateCurrentUnitCard();
        UpdateAvailableActions();
        UpdateEnemyUnitsPanel();
    }

    public void OnCellCLick()
    {
        var selectedCell = EventSystem.current.currentSelectedGameObject;
        // Если уже существует текущая клетка (подсвечивается зеленым), то назначаем targetCell
        if (m_CurrentCell != null && selectedCell != m_CurrentCell && !(_unitsPositions[m_CurrentCell].inBattleInfo.IsUsedAbility & _unitsPositions[m_CurrentCell].inBattleInfo.IsMoved))
        {
            m_TargetCell = selectedCell;

            m_AvailableCells = new List<GameObject>();
            m_ReachableCells = new List<GameObject>();
        }
        else
        {
            // Если игрок выбрал текущую клетку (произошел даблклик) -- обнуляем currentCell и targetCell 
            if (selectedCell == m_CurrentCell)
            {
                m_CurrentUnit.uiController.DisableHighlight();
                
                m_CurrentCell = null;
                m_TargetCell = null;

                m_AvailableCells = new List<GameObject>();
                m_ReachableCells = new List<GameObject>();
            }
            else
            {
                // Если текущая клетка ещё не назначена, то назначаем выбранную клетку текущей (если в ней находится союзный юнит)
                if (_unitsPositions.Keys.Contains(selectedCell) && _unitsPositions[selectedCell].inBattleInfo.IsAlly)
                {
                    m_CurrentUnit = _unitsPositions[selectedCell];
                    m_CurrentCell = selectedCell;
                    
                    m_CurrentUnit.uiController.EnableHighlight();

                    if (!m_CurrentUnit.inBattleInfo.IsMoved)
                        m_AvailableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.abilities.primaryMoveSkill.range);
                    else if (!m_CurrentUnit.inBattleInfo.IsUsedAbility)
                        m_ReachableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.abilities.primaryAttackSkill.range);
                }
            }
        }
    }

    private List<GameObject> GetAvailableCells(GameObject start, int range)
    {
        var result = new List<GameObject>();
        var barriers = GetUnitsAsPoints();
        foreach (var cell in m_CellsGrid)
        {
            if (barriers.Contains(GameObjectToPoint(cell))) continue;
            var currentPosition = GameObjectToPoint(start);
            var targetPosition = GameObjectToPoint(cell);
            var path = Bts(barriers, currentPosition, targetPosition);
            if (path.Count <= range)
            {
                foreach (var point in path)
                {
                    var cellAtPoint = m_CellsGrid[point.X, point.Y];
                    result.Add(cellAtPoint);
                }
            }
        }

        return result;
    }

    // Требует доработки
    // --- TODO: Вынести функционал в отдельный класс, привязанный к префабу юнита
    private void UpdateCells()
    {
        foreach (var cell in m_CellsGrid)
        {
            cell.transform.Find("OnReachable").gameObject.SetActive(m_ReachableCells.Contains(cell));
            cell.transform.Find("OnAvailable").gameObject.SetActive(m_AvailableCells.Contains(cell));
            cell.transform.Find("OnTarget").gameObject.SetActive(cell == m_TargetCell);
        }
    }

    private void UpdateCurrentUnitCard()
    {
        if (m_CurrentCell != null)
        {
            currentUnitCard.SetActive(true);
            currentUnitCard.transform.Find("Icon").GetComponent<Image>().sprite = m_CurrentUnit.transform.Find("Visual").Find("Icon").GetComponent<Image>().sprite;
            currentUnitCard.transform.Find("Health").GetChild(1).GetComponent<Text>().text = m_CurrentUnit.stats.Health.ToString();
            currentUnitCard.transform.Find("Armor").GetChild(1).GetComponent<Text>().text = m_CurrentUnit.stats.Armor.ToString();
        }
    }

    public void OnMoveButton()
    {
        var newInfo = m_CurrentUnit.abilities.primaryMoveSkill.Use(GetBattleInfo());
        UpdateBattleInfo(newInfo);
    }

    private List<Point> GetUnitsAsPoints(params GameObject[] exclude)
    {
        var result = new List<Point>();
        foreach (GameObject cell in _unitsPositions.Keys.Where(x => !exclude.Contains(x)))
        {
            Point p = GameObjectToPoint(cell);
            result.Add(p);
        }

        return result;
    }

    private Point GameObjectToPoint(GameObject cell)
    {
        // Парсим имя клетки формата 4_7 в точку Point(4,7)
        return new Point(int.Parse(cell.name.Split('_')[0]), int.Parse(cell.name.Split('_')[1]));
    }

    public void FirstAbilityButton()
    {
        Point currentPosition = GameObjectToPoint(m_CurrentCell);
        Point targetPosition = GameObjectToPoint(m_TargetCell);
        List<Point> path = Bts(new List<Point>(), currentPosition, targetPosition);
        if (_unitsPositions.ContainsKey(m_TargetCell) && !_unitsPositions[m_TargetCell].inBattleInfo.IsAlly & !_unitsPositions[m_CurrentCell].inBattleInfo.IsUsedAbility & path.Count <= m_CurrentUnit.abilities.primaryAttackSkill.range)
        {
            var newInfo = m_CurrentUnit.abilities.primaryAttackSkill.Use(GetBattleInfo());

            m_TargetCell = null;
            
            UpdateBattleInfo(newInfo);
            SwitchToNextUnit();
        }
    }

    // Функционал второй кнопки способностей
    public void SecondAbilityButton()
    {
        var newInfo = m_CurrentUnit.abilities.otherAbilities[0].Use(GetBattleInfo());
        UpdateBattleInfo(newInfo);

        m_TargetCell = null;

        SwitchToNextUnit();
    }

    public void ThirdAbilityButton()
    {
        var newInfo = m_CurrentUnit.abilities.otherAbilities[1].Use(GetBattleInfo());
        UpdateBattleInfo(newInfo);

        m_TargetCell = null;

        SwitchToNextUnit();
    }

    private void SwitchToNextUnit()
    {
        m_CurrentUnit = _unitsPositions.Values.FirstOrDefault(unit => !unit.inBattleInfo.IsUsedAbility && unit.inBattleInfo.IsAlly);
        m_CurrentCell = _unitsPositions.FirstOrDefault(x => x.Value == m_CurrentUnit).Key;

        if (!_unitsPositions[m_CurrentCell].inBattleInfo.IsAlly)
        {
            m_CurrentUnit = null;
            m_CurrentCell = null;
        }
        else
        {
            if (!m_CurrentUnit.inBattleInfo.IsMoved)
                m_AvailableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.abilities.primaryMoveSkill.range);
            m_CurrentUnit.uiController.EnableHighlight();
        }
        m_TargetCell = null;
    }

    private void UpdateAvailableActions()
    {
        if (m_CurrentCell == null)
        {
            abilitiesPanel.SetActive(false);
        }
        else
        {
            abilitiesPanel.SetActive(true);
            abilitiesPanel.transform.GetChild(1).GetComponent<Button>().enabled = !m_CurrentUnit.inBattleInfo.IsMoved;
            abilitiesPanel.transform.GetChild(1).GetChild(2).gameObject.SetActive(m_CurrentUnit.inBattleInfo.IsMoved);

            Transform abilitiesPanelTransform = abilitiesPanel.transform.Find("ActiveAbilities");

            abilitiesPanelTransform.GetChild(0).GetComponent<Button>().enabled = !m_CurrentUnit.inBattleInfo.IsUsedAbility;
            abilitiesPanelTransform.GetChild(0).GetChild(2).gameObject.SetActive(m_CurrentUnit.inBattleInfo.IsUsedAbility);

            abilitiesPanelTransform.GetChild(1).GetComponent<Button>().enabled = !m_CurrentUnit.inBattleInfo.IsUsedAbility;
            abilitiesPanelTransform.GetChild(1).GetChild(2).gameObject.SetActive(m_CurrentUnit.inBattleInfo.IsUsedAbility);

            abilitiesPanelTransform.GetChild(2).GetComponent<Button>().enabled = !m_CurrentUnit.inBattleInfo.IsUsedAbility;
            abilitiesPanelTransform.GetChild(2).GetChild(2).gameObject.SetActive(m_CurrentUnit.inBattleInfo.IsUsedAbility);

            UpdateAbilitiesPanel();

            foreach (Transform child in abilitiesPanel.transform)
            {
                child.gameObject.SetActive(m_CurrentCell != null);
            }
        }
    }

    private void UpdateAbilitiesPanel()
    {
        // Микро-тестик
        var moveParent = abilitiesPanel.transform.Find("Move");
        moveParent.Find("Image").GetComponent<Image>().sprite = m_CurrentUnit.abilities.primaryMoveSkill.sprite;

        var abilitiesParent = abilitiesPanel.transform.Find("ActiveAbilities");
        abilitiesParent.GetChild(0).Find("Text").GetComponent<Text>().text = m_CurrentUnit.abilities.primaryAttackSkill.abilityName;
        abilitiesParent.GetChild(0).Find("Image").GetComponent<Image>().sprite = m_CurrentUnit.abilities.primaryAttackSkill.sprite;

        for (int i = 0; i < m_CurrentUnit.abilities.otherAbilities.Count; i++)
        {
            abilitiesParent.GetChild(i + 1).Find("Text").GetComponent<Text>().text = m_CurrentUnit.abilities.otherAbilities[i].abilityName;
            abilitiesParent.GetChild(i + 1).Find("Image").GetComponent<Image>().sprite = m_CurrentUnit.abilities.otherAbilities[i].sprite;
        }
    }

    private void UpdateEnemyUnitsPanel()
    {
        foreach (Transform child in enemyUnitsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var unit in m_EnemySquad.Where(x => x.stats.Health > 0))
        {
            GameObject card = Instantiate(enemyCardPrefab, enemyUnitsPanel.transform.position, Quaternion.identity, enemyUnitsPanel.transform);
            card.transform.Find("Icon").GetComponent<Image>().sprite = unit.transform.Find("Visual").Find("Icon").GetComponent<Image>().sprite;
            card.transform.Find("Health").Find("Value").GetComponent<Text>().text = unit.stats.Health.ToString();
            card.transform.Find("Armor").Find("Value").GetComponent<Text>().text = unit.stats.Armor.ToString();
        }
    }

    private void SpawnUnitsOnField()
    {
        var exitPosition = new Point(0, 7);
        var allyPositions = GetRandomPointsArray(m_AllySquad.Count, new Point(0, 3), new Point(4, 7), new List<Point> { exitPosition });
        var enemyPositions = GetRandomPointsArray(m_EnemySquad.Count, new Point(3, 0), new Point(7, 4), allyPositions.Values.Append(exitPosition).ToList());

        m_ExitCell = m_CellsGrid[exitPosition.X, exitPosition.Y];
        m_ExitCell.transform.Find("Exit").gameObject.SetActive(true);

        for (var i = 0; i < m_AllySquad.Count; i++)
        {
            //var allyPrefab = unitPrefab;
            //allyPrefab.transform.Find("Icon").GetComponent<Image>().sprite = m_AllySquad[i].Sprite;
            
            var unit = SpawnUnit(m_AllySquad[i].gameObject, allyPositions[i], true);
            _unitsPositions.Add(m_CellsGrid[allyPositions[i].X, allyPositions[i].Y], unit.GetComponent<BattleUnit>());
            m_AllySquad[i] = unit.GetComponent<BattleUnit>();
        }

        for (var i = 0; i < m_EnemySquad.Count; i++)
        {
            //var enemyPrefab = unitPrefab;
            //enemyPrefab.transform.Find("Icon").GetComponent<Image>().sprite = m_EnemySquad[i].Sprite;
            
            var unit = SpawnUnit(m_EnemySquad[i].gameObject, enemyPositions[i], false);
            _unitsPositions.Add(m_CellsGrid[enemyPositions[i].X, enemyPositions[i].Y], unit.GetComponent<BattleUnit>());
            m_EnemySquad[i] = unit.GetComponent<BattleUnit>();
        }
    }

    // Добавил аргумент isAlly
    private GameObject SpawnUnit(GameObject prefab, Point position, bool isAlly)
    {
        var unit = Instantiate(prefab,
            m_CellsGrid[position.X, position.Y].transform.position, Quaternion.identity);
        unit.GetComponent<BattleUnit>().inBattleInfo.IsAlly = isAlly;
        unit.transform.SetParent(m_CellsGrid[position.X, position.Y].transform);

        return unit;
    }

    private void DrawCells()
    {
        if (cellPrefab == null)
        {
            Debug.Log("Script can not found cell prefab.");
            return;
        }

        var rectPrefab = (RectTransform)cellPrefab.transform;
        var cellSize = rectPrefab.rect.width;

        m_CellsGrid = new GameObject[Width, Height];
        var startPos = cellsParent.transform.position + new Vector3(cellSize / 2, cellSize / 2);

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

    private void CreateEnemySquad()
    {
        m_EnemySquad = new List<BattleUnit>
        {
            enemyInfantrymanPrefab.GetComponent<BattleUnit>(),
            enemySniperPrefab.GetComponent<BattleUnit>(),
            enemySwordsmanPrefab.GetComponent<BattleUnit>()
        };
    }

    public void EndTurn()
    {
        foreach (var unit in _unitsPositions.Values)
        {
            unit.inBattleInfo.RefreshAbilitiesAndMoving();
        }

        m_ReachableCells = new List<GameObject>();
        m_AvailableCells = new List<GameObject>();

        SwitchToNextUnit();
    }

    private static List<Point> Bts(IEnumerable<Point> barriers, Point start, Point end)
    {
        var a = new bool[8, 8];
        var b = new Point[8, 8];

        var q = new Queue<Point>();

        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                a[i, j] = true;

        if (!(barriers is null))
            foreach (var barrier in barriers)
                a[barrier.X, barrier.Y] = false;

        q.Enqueue(start);
        a[start.X, start.Y] = false;
        b[start.X, start.Y] = start;

        while (!q.Peek().Equals(end))
        {
            var e = q.Dequeue();

            var points = new[]
            {
                new Point(e.X - 1, e.Y),
                new Point(e.X + 1, e.Y),
                new Point(e.X, e.Y - 1),
                new Point(e.X, e.Y + 1),
                new Point(e.X + 1, e.Y + 1),
                new Point(e.X + 1, e.Y - 1),
                new Point(e.X - 1, e.Y + 1),
                new Point(e.X - 1, e.Y - 1),
            };

            foreach (var point in points)
            {
                if (point.X >= 0 && point.X < 8 && point.Y >= 0 && point.Y < 8 && a[point.X, point.Y])
                {
                    q.Enqueue(point);
                    a[point.X, point.Y] = false;
                    var p1 = point.X;
                    var p2 = point.Y;
                    var e1 = e.X;
                    var e2 = e.Y;
                    b[p1, p2] = new Point(e1, e2);
                }
            }
        }

        var result = new List<Point> { end, b[end.X, end.Y] };

        while (true)
        {
            var value = result[result.Count - 1];
            if (value.Equals(start)) break;
            var x = value.X;
            var y = value.Y;
            var v = b[x, y];
            result.Add(v);
        }

        result.ToArray();
        result.Reverse();
        result.RemoveAt(0);

        return result;
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

    private BattleInfo GetBattleInfo()
    {
        var battleInfo = new BattleInfo();

        battleInfo._unitsPositions = _unitsPositions;
        battleInfo.m_AllySquad = m_AllySquad;
        battleInfo.m_EnemySquad = m_EnemySquad;
        battleInfo.m_CurrentUnit = m_CurrentUnit;

        battleInfo.m_CellsGrid = m_CellsGrid;
        battleInfo.m_AvailableCells = m_AvailableCells;
        battleInfo.m_ReachableCells = m_ReachableCells;
        battleInfo.m_CurrentCell = m_CurrentCell;
        battleInfo.m_TargetCell = m_TargetCell;
        battleInfo.m_ExitCell = m_ExitCell;

        return battleInfo;
    }

    private void UpdateBattleInfo(BattleInfo info)
    {
        _unitsPositions = info._unitsPositions;
        m_AllySquad = info.m_AllySquad;
        m_EnemySquad = info.m_EnemySquad;
        m_CurrentUnit = info.m_CurrentUnit;

        m_CellsGrid = info.m_CellsGrid;
        m_AvailableCells = info.m_AvailableCells;
        m_ReachableCells = info.m_ReachableCells;
        m_CurrentCell = info.m_CurrentCell;
        m_TargetCell = info.m_TargetCell;
        m_ExitCell = info.m_ExitCell;
    }
}

