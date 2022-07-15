using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Random = System.Random;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject currentUnitCard;
    [SerializeField] private GameObject abilitiesPanel;
    [SerializeField] private GameObject enemyUnitsPanel;
    [SerializeField] private GameObject passiveAbilitiesPopup;
    [SerializeField] private GameObject endTurnButton;
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject enemyCardPrefab;

    private static Dictionary<GameObject, Unit> _unitsPositions;
    private List<Unit> m_AllySquad;
    private List<Unit> m_EnemySquad;
    private Unit m_CurrentUnit;
    private Unit m_CurrentEnemyUnit;
    
    private GameObject[,] m_CellsGrid;
    private List<GameObject> m_AvailableCells;
    private List<GameObject> m_ReachableCells;
    private GameObject m_CurrentCell;
    private GameObject m_TargetCell;
    private GameObject m_ExitCell;
    private List<GameObject> m_ObstacleCells;

    private const int Width = 8;
    private const int Height = 8;

    private readonly Random m_Rnd = new Random();

    public void Awake()
    {
        Application.targetFrameRate = 30;
        
        var cellPrefabRect = cellPrefab.transform as RectTransform;
        cellPrefabRect.sizeDelta = new Vector2 (1.218f * 1080/Screen.height, 1.218f * 1080/Screen.height);
        var unitPrefabRect = unitPrefab.transform as RectTransform;
        unitPrefabRect.sizeDelta = new Vector2 (1.18f, 1.18f);
        _unitsPositions = new Dictionary<GameObject, Unit>();

        m_AllySquad = new List<Unit>
        {
            Swordsman.CreateInstance(true),
            Assault.CreateInstance(true),
            Sniper.CreateInstance(true)
        };

        DrawCells();
    }

    public void Start()
    {
        CreateEnemySquad();
        SpawnExitCell();
        SpawnObstacles();
        SpawnUnitsOnField();
        SwitchToNextUnit();
        UpdateEnemyUnitsPanel();
        m_ReachableCells = new List<GameObject>();
        m_AvailableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.MovingRange);
    }

    public void Update()
    {
        UpdateCells();
        UpdateCurrentUnitCard();
        UpdateAvailableAbilities();
    }

    public void OnCellCLick()
    {
        var selectedCell = EventSystem.current.currentSelectedGameObject;
        // Если уже существует текущая клетка (подсвечивается зеленым), то назначаем targetCell
        if (m_CurrentCell != null && selectedCell != m_CurrentCell && !(_unitsPositions[m_CurrentCell].IsUsedAbility & _unitsPositions[m_CurrentCell].IsMoved))
        {
            m_TargetCell = selectedCell;
            m_AvailableCells = new List<GameObject>();
            m_ReachableCells = new List<GameObject>();
        }
        else
        {
            // Если игрок выбрал текущаю клетку (произошел даблклик) -- обнуляем currentCell и targetCell 
            if (selectedCell == m_CurrentCell)
            {
                m_CurrentCell = null;
                m_TargetCell = null;
                m_AvailableCells = new List<GameObject>();
                m_ReachableCells = new List<GameObject>();
            }
            else
            {
                // Если текущая клетка ещё не назначена, то назначаем выбранную клетку текущей (если в ней находится союзный юнит)
                if (_unitsPositions.Keys.Contains(selectedCell) && _unitsPositions[selectedCell].IsAlly)
                {
                    m_CurrentUnit = _unitsPositions[selectedCell];
                    m_CurrentCell = selectedCell;

                    if (!m_CurrentUnit.IsMoved)
                        m_AvailableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.MovingRange);
                    else if (!m_CurrentUnit.IsUsedAbility)
                        m_ReachableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.AttackRange);
                }
            }
        }
    }
    
    private List<GameObject> GetAvailableCells(GameObject start, int range)
    {
        var result = new List<GameObject>();
        var barriers = GetBarriers();
        foreach (var cell in m_CellsGrid)
        {
            if (barriers.Contains(GameObjectToPoint(cell))) continue;
            var currentPosition = GameObjectToPoint(start);
            var targetPosition = GameObjectToPoint(cell);
            var path = GetPath(barriers, currentPosition, targetPosition);
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
    
    private void UpdateCells()
    {
        foreach (var cell in m_CellsGrid)
        {
            if (_unitsPositions.ContainsKey(cell))
            {
                cell.transform.Find("Unit(Clone)").Find("OnActive").gameObject.SetActive(false);
                cell.transform.Find("Unit(Clone)").Find("OnActiveBackground").gameObject.SetActive(false);
            }
            cell.transform.Find("OnAvailable").gameObject.SetActive(m_AvailableCells.Contains(cell));
            cell.transform.Find("OnReachable").gameObject.SetActive(m_ReachableCells.Contains(cell));
            cell.transform.Find("OnTarget").gameObject.SetActive(cell == m_TargetCell);
        }

        if (m_CurrentCell != null)
        {
            m_CurrentCell.transform.Find("Unit(Clone)").Find("OnActive").gameObject.SetActive(true);
            m_CurrentCell.transform.Find("Unit(Clone)").Find("OnActiveBackground").gameObject.SetActive(true);
        }

        if (m_AllySquad.Count == 0 || m_EnemySquad.Count == 0)
        {
            SceneManager.LoadScene("Map");
        }
    }

    private void UpdateCurrentUnitCard()
    {
        if (m_CurrentCell != null)
        {
            currentUnitCard.SetActive(true);
            try
            {
                currentUnitCard.transform.Find("Icon").GetComponent<Image>().sprite = m_CurrentUnit.Sprite;
                currentUnitCard.transform.Find("Health").GetChild(1).GetComponent<Text>().text =
                    m_CurrentUnit.Health.ToString();
                currentUnitCard.transform.Find("Armor").GetChild(1).GetComponent<Text>().text =
                    m_CurrentUnit.Armor.ToString();
            }
            catch (NullReferenceException e)
            {
                currentUnitCard.SetActive(false);
            }
        }
    }

    public void OnMoveButton()
    {
        GameObject unitAsGameObject = m_CurrentCell.transform.Find("Unit(Clone)").gameObject;
        List<Point> barriers = GetBarriers(m_CurrentCell, m_TargetCell);
        Point currentPosition = GameObjectToPoint(m_CurrentCell);
        Point targetPosition = GameObjectToPoint(m_TargetCell);
        List<Point> path = GetPath(barriers, currentPosition, targetPosition);

        if (m_CurrentUnit.MovingRange < path.Count)
        {
            Debug.Log("Слишком далеко, ты не можешь туда сходить!");
            return;
        }

        if (!_unitsPositions.ContainsKey(m_TargetCell) && m_TargetCell != m_ExitCell && !_unitsPositions[m_CurrentCell].IsMoved)
        {
            _unitsPositions[m_TargetCell] = m_CurrentUnit;
            _unitsPositions.Remove(m_CurrentCell);

            m_CurrentCell = m_TargetCell;
            m_TargetCell = null;
            
            StartCoroutine(Move(unitAsGameObject, path));
            _unitsPositions[m_CurrentCell].Moved();
        }
        else if (m_TargetCell == m_ExitCell)
        {
            m_AllySquad.Remove(_unitsPositions[m_CurrentCell]);
            _unitsPositions.Remove(m_CurrentCell);
            
            StartCoroutine(Move(unitAsGameObject, path));
            Destroy(unitAsGameObject);

            if (m_AllySquad.Count != 0)
            {
                SwitchToNextUnit();
            }
        }
        else
        {
            Debug.Log("Ты не можешь сходить туда!");
        }
    }
    
    private IEnumerator Move(GameObject unit, List<Point> path)
    {
        foreach (var point in path)
        {
            unit.transform.position = m_CellsGrid[point.X, point.Y].transform.position;
            unit.transform.parent = m_CellsGrid[point.X, point.Y].transform;
            yield return new WaitForSeconds(0.2f);
        }

        if (m_CurrentCell != null)
            m_ReachableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.AttackRange);
    }

    private List<Point> GetBarriers(params GameObject[] exclude)
    {
        var result = new List<Point>();
        foreach (GameObject cell in _unitsPositions.Keys.Where(x => !exclude.Contains(x)))
        {
            result.Add(GameObjectToPoint(cell));
        }
        foreach (GameObject obstacle in m_ObstacleCells.Where(x => !exclude.Contains(x)))
        {
            result.Add(GameObjectToPoint(obstacle));
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
        List<Point> path = GetPath(GetBarriers(m_CurrentCell, m_TargetCell), currentPosition, targetPosition);
        if (_unitsPositions.ContainsKey(m_TargetCell) && !_unitsPositions[m_TargetCell].IsAlly & !_unitsPositions[m_CurrentCell].IsUsedAbility & path.Count <= m_CurrentUnit.AttackRange)
        {
            var wasHealth = _unitsPositions[m_TargetCell].Armor + _unitsPositions[m_TargetCell].Health;
            m_CurrentUnit.Ability1(_unitsPositions[m_TargetCell]);
            StartCoroutine(ShowDamage(m_TargetCell.transform.Find("Unit(Clone)").gameObject, wasHealth - _unitsPositions[m_TargetCell].Health));

            if (_unitsPositions[m_TargetCell].Health <= 0)
            {
                Destroy(m_TargetCell.transform.Find("Unit(Clone)").gameObject);
                _unitsPositions.Remove(m_TargetCell);
            }
            m_TargetCell = null;
            
            UpdateEnemyUnitsPanel();
            SwitchToNextUnit();
        }
    }
    
    private void SwitchToNextUnit()
    {
        m_CurrentUnit = _unitsPositions.FirstOrDefault(x => x.Value.IsAlly && !x.Value.IsMoved).Value;
        m_CurrentCell = _unitsPositions.FirstOrDefault(x => x.Value.GetHashCode() == m_CurrentUnit.GetHashCode()).Key;
        m_TargetCell = null;

        if (!m_CurrentUnit.IsMoved)
        {
            m_AvailableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.MovingRange);
        }
        else if (!m_CurrentUnit.IsUsedAbility)
        {
            m_ReachableCells = GetAvailableCells(m_CurrentCell, m_CurrentUnit.AttackRange);
        }
        else
        {
            m_CurrentCell = null;
            m_AvailableCells = new List<GameObject>();
            m_ReachableCells = new List<GameObject>();
        }
    }

    private void UpdateAvailableAbilities()
    {
        if (m_CurrentCell == null)
        {
            abilitiesPanel.SetActive(false);
        }
        else
        {
            abilitiesPanel.SetActive(true);
            abilitiesPanel.transform.GetChild(1).GetComponent<Button>().enabled = !m_CurrentUnit.IsMoved;
            abilitiesPanel.transform.GetChild(1).GetChild(2).gameObject.SetActive(m_CurrentUnit.IsMoved);
            
            abilitiesPanel.transform.Find("ActiveAbilities").GetChild(0).GetComponent<Button>().enabled = !m_CurrentUnit.IsUsedAbility;
            abilitiesPanel.transform.Find("ActiveAbilities").GetChild(0).GetChild(2).gameObject.SetActive(m_CurrentUnit.IsUsedAbility);
            // Затемнение неспользуемых кнопок способностей
            abilitiesPanel.transform.Find("ActiveAbilities").GetChild(1).GetChild(2).gameObject.SetActive(true);
            abilitiesPanel.transform.Find("ActiveAbilities").GetChild(2).GetChild(2).gameObject.SetActive(true);
            abilitiesPanel.transform.Find("ActiveAbilities").GetChild(3).GetChild(2).gameObject.SetActive(true);
            UpdateAbilitiesPanel();
            foreach (Transform child in abilitiesPanel.transform)
            {
                child.gameObject.SetActive(m_CurrentCell != null);
            }
        }
    }

    private void UpdateAbilitiesPanel()
    {
        var activeAbilitiesParent = abilitiesPanel.transform.Find("ActiveAbilities");
        for(var i = 0; i < activeAbilitiesParent.childCount; i++)
        {
            activeAbilitiesParent.GetChild(i).Find("Text").GetComponent<Text>().text = m_CurrentUnit.ActiveAbilitiesNames[i];
            activeAbilitiesParent.GetChild(i).Find("Image").GetComponent<Image>().sprite = m_CurrentUnit.ActiveAbilitiesIcons[i];
        }
        
        var passiveAbilitiesParent = abilitiesPanel.transform.Find("PassiveAbilities");
        for(var i = 0; i < passiveAbilitiesParent.childCount; i++)
        {
            passiveAbilitiesParent.GetChild(i).GetComponent<Image>().sprite = m_CurrentUnit.PassiveAbilitiesIcons[i];
        }
    }

    private void UpdateEnemyUnitsPanel()
    {
        foreach (Transform child in enemyUnitsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var unit in m_EnemySquad.Where(x => x.Health > 0))
        {
            GameObject card = Instantiate(enemyCardPrefab, enemyUnitsPanel.transform.position, Quaternion.identity, enemyUnitsPanel.transform);
            card.transform.Find("Icon").GetComponent<Image>().sprite = unit.Sprite;
            card.transform.Find("Health").Find("Value").GetComponent<Text>().text = unit.Health.ToString();
            card.transform.Find("Armor").Find("Value").GetComponent<Text>().text = unit.Armor.ToString();
        }
    }

    private void SpawnUnitsOnField()
    {
        var obstacles = GetBarriers();
        var allyPositions = GetRandomPointsArray(m_AllySquad.Count, new Point(0,4), new Point(4, 7), obstacles);
        var enemyPositions = GetRandomPointsArray(m_EnemySquad.Count, new Point(4,0), new Point(7, 4), obstacles);

        for (var i=0; i<m_AllySquad.Count; i++)
        {
            var allyPrefab = unitPrefab;
            allyPrefab.transform.Find("Icon").GetComponent<Image>().sprite = m_AllySquad[i].Sprite;
            _unitsPositions.Add(m_CellsGrid[allyPositions[i].X, allyPositions[i].Y], m_AllySquad[i]);
            
            SpawnUnit(allyPrefab, allyPositions[i]);
        }
        for (var i=0; i<m_EnemySquad.Count; i++)
        {
            var enemyPrefab = unitPrefab;
            enemyPrefab.transform.Find("Icon").GetComponent<Image>().sprite = m_EnemySquad[i].Sprite;
            _unitsPositions.Add(m_CellsGrid[enemyPositions[i].X, enemyPositions[i].Y], m_EnemySquad[i]);
            
            SpawnUnit(enemyPrefab, enemyPositions[i]);
        }
    }

    private void SpawnExitCell()
    {
        var exitPosition = new Point(0, 7);
        m_ExitCell = m_CellsGrid[exitPosition.X, exitPosition.Y];
        m_ExitCell.transform.Find("Exit").gameObject.SetActive(true);
    }

    private void SpawnObstacles()
    {
        m_ObstacleCells = new List<GameObject>();
        var obstaclesPositions = GetRandomPointsArray(4, new Point(1,1), new Point(7, 7), new List<Point>{GameObjectToPoint(m_ExitCell)});
        
        for (var i=0; i<obstaclesPositions.Count; i++)
        {
            GameObject obstacleCell = m_CellsGrid[obstaclesPositions[i].X, obstaclesPositions[i].Y];
            obstacleCell.transform.Find("Obstacle_1").gameObject.SetActive(true);
            m_ObstacleCells.Add(obstacleCell);
        }
    }

    private void SpawnUnit(GameObject prefab, Point position)
    {
        var unit = Instantiate(prefab,
            m_CellsGrid[position.X, position.Y].transform.position, Quaternion.identity);
        unit.transform.SetParent(m_CellsGrid[position.X, position.Y].transform);
    }
    
    public void EndTurn()
    {
        foreach (var unit in _unitsPositions.Values)
        {
            unit.RefreshAbilitiesAndMoving();
        }
        m_ReachableCells = new List<GameObject>();
        m_AvailableCells = new List<GameObject>();
        m_CurrentUnit = null;
        m_CurrentCell = null;
        m_TargetCell = null;
        UpdateCells();
        StartCoroutine(EnemiesTurn());
    }

    private IEnumerator EnemiesTurn()
    {
        endTurnButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Battle Screen UI Elements/Turn Button Inactive");
        foreach (Unit enemy in m_EnemySquad)
        {
            if (enemy.GetHashCode() != m_EnemySquad.First().GetHashCode())
            {
                enemyUnitsPanel.transform.GetChild(0).SetSiblingIndex(2);
            }
            (Unit ally, List<Point> path) = GetClosestAlly(enemy);
            var allyCell = m_CellsGrid[path.Last().X, path.Last().Y];
            var enemyCell = _unitsPositions.FirstOrDefault(x => x.Value.GetHashCode() == enemy.GetHashCode()).Key;
            var enemyAsGameObject = enemyCell.transform.Find("Unit(Clone)").gameObject;
            var allyAsGameObject = allyCell.transform.Find("Unit(Clone)").gameObject;
            
            // Если не получается атаковать
            if (path.Count > enemy.AttackRange && path.Count > 1)
            {
                // Удаляем часть пути, чтобы атаковать на дистанции
                path.RemoveRange(path.Count - enemy.AttackRange, enemy.AttackRange);
                _unitsPositions.Remove(enemyCell);
                _unitsPositions[m_CellsGrid[path.Last().X, path.Last().Y]] = enemy;
                _unitsPositions[m_CellsGrid[path.Last().X, path.Last().Y]].Moved();
                StartCoroutine(Move(enemyAsGameObject, path));
            }
            // Если атаковать возможно
            if (path.Count <= enemy.AttackRange)
            {
                var wasHealth = ally.Damage + ally.Health;
                enemy.Ability1(ally);
                StartCoroutine(ShowDamage(allyAsGameObject, wasHealth - ally.Health));
                if (ally.Health <= 0)
                {
                    Destroy(allyCell.transform.Find("Unit(Clone)").gameObject);
                    m_AllySquad.Remove(ally);
                    _unitsPositions.Remove(allyCell);
                }
                Debug.Log(enemy.Name + " " + (wasHealth - ally.Health));
            }
            yield return new WaitForSeconds(2);
        }
        endTurnButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Battle Screen UI Elements/Turn Button");
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator ShowDamage(GameObject unitAsGameObject, int damage)
    {
        var damagePrefab = unitAsGameObject.transform.Find("Damage").Find("HealthMinus");
        if (damage > 0)
        {
            damagePrefab.GetComponent<Text>().text = '-' + damage.ToString();
        }
        else
        {
            damagePrefab.GetComponent<Text>().text = "Промах!";
        }
        damagePrefab.gameObject.SetActive(true);
        
        StartCoroutine(FadeTextToFullAlpha(0.2f,damagePrefab.GetComponent<Text>()));
        yield return new WaitForSeconds(3);
        StartCoroutine(FadeTextToZeroAlpha(0.2f,damagePrefab.GetComponent<Text>()));
        damagePrefab.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator FadeTextToFullAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    private IEnumerator FadeTextToZeroAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    private Tuple<Unit, List<Point>> GetClosestAlly(Unit unit)
    {
        var paths = new Dictionary<Unit, List<Point>>();
        for (int i=0; i<m_AllySquad.Count; i++)
        {
            GameObject startCell = _unitsPositions.FirstOrDefault(x => x.Value.GetHashCode() == unit.GetHashCode()).Key;
            Point start = GameObjectToPoint(startCell);
            GameObject endCell = _unitsPositions.FirstOrDefault(x => x.Value.GetHashCode() == m_AllySquad[i].GetHashCode()).Key;
            if (endCell == null)
            {
                Debug.Log("Can't find cell with " + m_AllySquad[i].Name);
                continue;
            }
            Point end = GameObjectToPoint(endCell);
            var pathToUnit = GetPath(GetBarriers(startCell, endCell), start, end);
            paths[m_AllySquad[i]] = pathToUnit;
        }
        if(m_AllySquad.Count == 1) return Tuple.Create(paths.First().Key, paths.First().Value);
        var sortedPaths = from entry in paths orderby entry.Value.Count ascending select entry;
        return Tuple.Create(sortedPaths.First().Key, sortedPaths.First().Value);
    }

    public void OpenPassiveAbilitiesPopup()
    {
        var parent = passiveAbilitiesPopup.transform.Find("Panel").Find("ColumnsPassiveAbilities");
        for (var i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).Find("Text").GetComponent<Text>().text = m_CurrentUnit.PassiveAbilitiesDescriptions[i];
            parent.GetChild(i).Find("Title").Find("Image").GetComponent<Image>().sprite =
                m_CurrentUnit.PassiveAbilitiesIcons[i];
            parent.GetChild(i).Find("Title").Find("TitleAbility").Find("Text").GetComponent<Text>().text =
                m_CurrentUnit.PassiveAbilitiesNames[i];
        }
        passiveAbilitiesPopup.SetActive(true);
    }
    
    public void ClosePassiveAbilitiesPopup()
    {
        passiveAbilitiesPopup.SetActive(false);
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

    private void CreateEnemySquad()
    {
        m_EnemySquad = new List<Unit>
        {
            Assault.CreateInstance(false),
            Swordsman.CreateInstance(false),
            Sniper.CreateInstance(false)
        };
    }

    private static List<Point> GetPath(IEnumerable<Point> barriers, Point start, Point end)
    {
        var a = new bool[8, 8];
        var b = new Point[8, 8];

        var q = new Queue<Point>();

        for (int i = 0; i < 8; i++)
        for (int j = 0; j < 8; j++)
            a[i, j] = true;

        if (barriers != null)
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

        var result = new List<Point> {end, b[end.X, end.Y]};

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
}
