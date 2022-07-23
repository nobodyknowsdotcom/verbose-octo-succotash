using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Entities;
using Unity.Mathematics;
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
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject enemyCardPrefab;
    
    [SerializeField] private GameObject allyInfantrymanPrefab;
    [SerializeField] private GameObject allySniperPrefab;
    [SerializeField] private GameObject allySwordsmanPrefab;
    [SerializeField] private GameObject enemyInfantrymanPrefab;
    [SerializeField] private GameObject enemySniperPrefab;
    [SerializeField] private GameObject enemySwordsmanPrefab;

    private static Dictionary<GameObject, BattleUnit> _unitsPositions;
    private List<BattleUnit> _allySquad;
    private List<BattleUnit> _enemySquad;
    private BattleUnit _currentUnit;

    private GameObject[,] _cellsGrid;
    private List<GameObject> _availableCells;
    private List<GameObject> _reachableCells;
    private GameObject _currentCell;
    private GameObject _targetCell;
    public List<Obstacle> obstacles;
    private GameObject _exitCell;

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
        _allySquad = new List<BattleUnit>
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

        _availableCells = GetAvailableCells(_currentCell, _currentUnit.abilities.primaryMoveSkill.range);
        _reachableCells = new List<GameObject>();
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
        if (_currentCell != null && selectedCell != _currentCell && !(_unitsPositions[_currentCell].inBattleInfo.IsUsedAbility & _unitsPositions[_currentCell].inBattleInfo.IsMoved))
        {
            _targetCell = selectedCell;

            _availableCells = new List<GameObject>();
            _reachableCells = new List<GameObject>();
        }
        else
        {
            // Если игрок выбрал текущую клетку (произошел даблклик) -- обнуляем currentCell и targetCell 
            if (selectedCell == _currentCell)
            {
                _currentUnit.uiController.DisableHighlight();
                
                _currentCell = null;
                _targetCell = null;

                _availableCells = new List<GameObject>();
                _reachableCells = new List<GameObject>();
            }
            else
            {
                // Если текущая клетка ещё не назначена, то назначаем выбранную клетку текущей (если в ней находится союзный юнит)
                if (_unitsPositions.Keys.Contains(selectedCell) && _unitsPositions[selectedCell].inBattleInfo.IsAlly)
                {
                    _currentUnit = _unitsPositions[selectedCell];
                    _currentCell = selectedCell;
                    
                    _currentUnit.uiController.EnableHighlight();

                    if (!_currentUnit.inBattleInfo.IsMoved)
                        _availableCells = GetAvailableCells(_currentCell, _currentUnit.abilities.primaryMoveSkill.range);
                    else if (!_currentUnit.inBattleInfo.IsUsedAbility)
                        _reachableCells = GetAvailableCells(_currentCell, _currentUnit.abilities.primaryAttackSkill.range);
                }
            }
        }
    }

    private List<GameObject> GetAvailableCells(GameObject start, int range)
    {
        var result = new List<GameObject>();
        var barriers = GetUnitsAsPoints();
        foreach (var cell in _cellsGrid)
        {
            if (barriers.Contains(GameObjectToPoint(cell))) continue;
            var currentPosition = GameObjectToPoint(start);
            var targetPosition = GameObjectToPoint(cell);
            var path = Bts(barriers, currentPosition, targetPosition);
            if (path.Count <= range)
            {
                foreach (var point in path)
                {
                    var cellAtPoint = _cellsGrid[point.X, point.Y];
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
        foreach (var cell in _cellsGrid)
        {
            cell.transform.Find("OnReachable").gameObject.SetActive(_reachableCells.Contains(cell));
            cell.transform.Find("OnAvailable").gameObject.SetActive(_availableCells.Contains(cell));
            cell.transform.Find("OnTarget").gameObject.SetActive(cell == _targetCell);
        }
    }

    private void UpdateCurrentUnitCard()
    {
        if (_currentCell != null)
        {
            currentUnitCard.SetActive(true);
            currentUnitCard.transform.Find("Icon").GetComponent<Image>().sprite = _currentUnit.transform.Find("Visual").Find("Icon").GetComponent<Image>().sprite;
            currentUnitCard.transform.Find("Health").GetChild(1).GetComponent<Text>().text = _currentUnit.stats.Health.ToString();
            currentUnitCard.transform.Find("Armor").GetChild(1).GetComponent<Text>().text = _currentUnit.stats.Armor.ToString();
        }
    }

    public void OnMoveButton()
    {
        var newInfo = _currentUnit.abilities.primaryMoveSkill.Use(GetBattleInfo());
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
        Point currentPosition = GameObjectToPoint(_currentCell);
        Point targetPosition = GameObjectToPoint(_targetCell);
        List<Point> path = Bts(new List<Point>(), currentPosition, targetPosition);
        if (_unitsPositions.ContainsKey(_targetCell) && !_unitsPositions[_targetCell].inBattleInfo.IsAlly & !_unitsPositions[_currentCell].inBattleInfo.IsUsedAbility & path.Count <= _currentUnit.abilities.primaryAttackSkill.range)
        {
            var newInfo = _currentUnit.abilities.primaryAttackSkill.Use(GetBattleInfo());

            _targetCell = null;
            
            UpdateBattleInfo(newInfo);
            SwitchToNextUnit();
        }
    }

    // Функционал второй кнопки способностей
    public void SecondAbilityButton()
    {
        var newInfo = _currentUnit.abilities.otherAbilities[0].Use(GetBattleInfo());
        UpdateBattleInfo(newInfo);

        _targetCell = null;

        SwitchToNextUnit();
    }

    public void ThirdAbilityButton()
    {
        var newInfo = _currentUnit.abilities.otherAbilities[1].Use(GetBattleInfo());
        UpdateBattleInfo(newInfo);

        _targetCell = null;

        SwitchToNextUnit();
    }
    
    public void FourthAbilityButton()
    {
        var newInfo = _currentUnit.abilities.otherAbilities[2].Use(GetBattleInfo());
        UpdateBattleInfo(newInfo);

        _targetCell = null;

        SwitchToNextUnit();
    }

    private void SwitchToNextUnit()
    {
        if (_currentUnit != null) _currentUnit.uiController.DisableHighlight();
        _currentUnit = _unitsPositions.Values.FirstOrDefault(unit => !unit.inBattleInfo.IsUsedAbility && unit.inBattleInfo.IsAlly);

        if (_currentUnit == null || !_currentUnit.inBattleInfo.IsAlly)
        {
            _currentUnit = null;
            _currentCell = null;
        }
        else
        {
            _currentCell = _unitsPositions.FirstOrDefault(x => x.Value == _currentUnit).Key;
            if (!_currentUnit.inBattleInfo.IsMoved)
                _availableCells = GetAvailableCells(_currentCell, _currentUnit.abilities.primaryMoveSkill.range);
            _currentUnit.uiController.EnableHighlight();
        }
        _targetCell = null;
    }

    private void UpdateAvailableActions()
    {
        if (_currentCell == null)
        {
            abilitiesPanel.SetActive(false);
        }
        else
        {
            abilitiesPanel.SetActive(true);
            abilitiesPanel.transform.GetChild(1).GetComponent<Button>().enabled = !_currentUnit.inBattleInfo.IsMoved;
            abilitiesPanel.transform.GetChild(1).GetChild(2).gameObject.SetActive(_currentUnit.inBattleInfo.IsMoved);

            Transform abilitiesPanelTransform = abilitiesPanel.transform.Find("ActiveAbilities");

            abilitiesPanelTransform.GetChild(0).GetComponent<Button>().enabled = !_currentUnit.inBattleInfo.IsUsedAbility;
            abilitiesPanelTransform.GetChild(0).GetChild(2).gameObject.SetActive(_currentUnit.inBattleInfo.IsUsedAbility);
            abilitiesPanelTransform.GetChild(1).GetComponent<Button>().enabled = !_currentUnit.inBattleInfo.IsUsedAbility;
            abilitiesPanelTransform.GetChild(1).GetChild(2).gameObject.SetActive(_currentUnit.inBattleInfo.IsUsedAbility);
            abilitiesPanelTransform.GetChild(2).GetComponent<Button>().enabled = !_currentUnit.inBattleInfo.IsUsedAbility;
            abilitiesPanelTransform.GetChild(2).GetChild(2).gameObject.SetActive(_currentUnit.inBattleInfo.IsUsedAbility);
            abilitiesPanelTransform.GetChild(3).GetComponent<Button>().enabled = !_currentUnit.inBattleInfo.IsUsedAbility;
            abilitiesPanelTransform.GetChild(3).GetChild(2).gameObject.SetActive(_currentUnit.inBattleInfo.IsUsedAbility);

            UpdateAbilitiesPanel();

            foreach (Transform child in abilitiesPanel.transform)
            {
                child.gameObject.SetActive(_currentCell != null);
            }
        }
    }

    private void UpdateAbilitiesPanel()
    {
        // Микро-тестик
        var moveParent = abilitiesPanel.transform.Find("Move");
        moveParent.Find("Image").GetComponent<Image>().sprite = _currentUnit.abilities.primaryMoveSkill.sprite;

        var abilitiesParent = abilitiesPanel.transform.Find("ActiveAbilities");
        abilitiesParent.GetChild(0).Find("Text").GetComponent<Text>().text = _currentUnit.abilities.primaryAttackSkill.abilityName;
        abilitiesParent.GetChild(0).Find("Image").GetComponent<Image>().sprite = _currentUnit.abilities.primaryAttackSkill.sprite;

        for (int i = 0; i < _currentUnit.abilities.otherAbilities.Count; i++)
        {
            abilitiesParent.GetChild(i + 1).Find("Text").GetComponent<Text>().text = _currentUnit.abilities.otherAbilities[i].abilityName;
            abilitiesParent.GetChild(i + 1).Find("Image").GetComponent<Image>().sprite = _currentUnit.abilities.otherAbilities[i].sprite;
        }
    }

    private void UpdateEnemyUnitsPanel()
    {
        foreach (Transform child in enemyUnitsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var unit in _enemySquad.Where(x => x.stats.Health > 0))
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
        SpawnObstacles(new List<Point>{exitPosition});
        var restrictedPoints = GetBattleInfo().GetBarriers();
        restrictedPoints.Add(exitPosition);
        var allyPositions = GetRandomPointsArray(_allySquad.Count, new Point(0, 3), new Point(4, 7), restrictedPoints);
        var enemyPositions = GetRandomPointsArray(_enemySquad.Count, new Point(3, 0), new Point(7, 4), allyPositions.Values.Concat(restrictedPoints).ToList());

        _exitCell = _cellsGrid[exitPosition.X, exitPosition.Y];
        _exitCell.transform.Find("Exit").gameObject.SetActive(true);

        for (var i = 0; i < _allySquad.Count; i++)
        {
            var unit = SpawnUnit(_allySquad[i].gameObject, allyPositions[i], true);
            _unitsPositions.Add(_cellsGrid[allyPositions[i].X, allyPositions[i].Y], unit.GetComponent<BattleUnit>());
            _allySquad[i] = unit.GetComponent<BattleUnit>();
        }

        for (var i = 0; i < _enemySquad.Count; i++)
        {
            var unit = SpawnUnit(_enemySquad[i].gameObject, enemyPositions[i], false);
            _unitsPositions.Add(_cellsGrid[enemyPositions[i].X, enemyPositions[i].Y], unit.GetComponent<BattleUnit>());
            _enemySquad[i] = unit.GetComponent<BattleUnit>();
        }
    }

    private void SpawnObstacles(List<Point> restrictedPoints)
    {
        var obstaclesPositions = GetRandomPointsArray(4, new Point(1,1), new Point(7, 7), restrictedPoints);
        for (var i=0; i<obstaclesPositions.Count; i++)
        {
            GameObject parent = _cellsGrid[obstaclesPositions[i].X, obstaclesPositions[i].Y];
            var obstacle = Instantiate(rockPrefab, parent.transform.position, quaternion.identity);
            obstacle.transform.SetParent(parent.transform);
            obstacle.GetComponent<Obstacle>().Position = new Point(obstaclesPositions[i].X, obstaclesPositions[i].Y);
            obstacles.Add(obstacle.GetComponent<Obstacle>());
        }
        UpdateBattleInfo(GetBattleInfo());
    }

    // Добавил аргумент isAlly
    private GameObject SpawnUnit(GameObject prefab, Point position, bool isAlly)
    {
        var unit = Instantiate(prefab,
            _cellsGrid[position.X, position.Y].transform.position, Quaternion.identity);
        unit.GetComponent<BattleUnit>().inBattleInfo.IsAlly = isAlly;
        unit.transform.SetParent(_cellsGrid[position.X, position.Y].transform);

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

        _cellsGrid = new GameObject[Width, Height];
        var startPos = cellsParent.transform.position + new Vector3(cellSize / 2, cellSize / 2);

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                _cellsGrid[i, j] = Instantiate(
                    cellPrefab,
                    startPos + new Vector3(i * cellSize, j * cellSize, 0),
                    Quaternion.identity);
                _cellsGrid[i, j].transform.SetParent(cellsParent.transform);
                _cellsGrid[i, j].gameObject.name = i + "_" + j;
                _cellsGrid[i, j].GetComponent<Button>().onClick.AddListener(OnCellCLick);
            }
        }
    }

    private void CreateEnemySquad()
    {
        _enemySquad = new List<BattleUnit>
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

        _reachableCells = new List<GameObject>();
        _availableCells = new List<GameObject>();

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

        battleInfo.UnitsPositions = _unitsPositions;
        battleInfo.allySquad = _allySquad;
        battleInfo.enemySquad = _enemySquad;
        battleInfo.currentUnit = _currentUnit;

        battleInfo.CellsGrid = _cellsGrid;
        battleInfo.availableCells = _availableCells;
        battleInfo.reachableCells = _reachableCells;
        battleInfo.currentCell = _currentCell;
        battleInfo.targetCell = _targetCell;
        battleInfo.obstacles = obstacles;
        battleInfo.exitCell = _exitCell;

        return battleInfo;
    }

    private void UpdateBattleInfo(BattleInfo info)
    {
        _unitsPositions = info.UnitsPositions;
        _allySquad = info.allySquad;
        _enemySquad = info.enemySquad;
        _currentUnit = info.currentUnit;

        _cellsGrid = info.CellsGrid;
        _availableCells = info.availableCells;
        _reachableCells = info.reachableCells;
        _currentCell = info.currentCell;
        _targetCell = info.targetCell;
        obstacles = info.obstacles;
        _exitCell = info.exitCell;
    }
}

