using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject currentWarriorCard;

    private static Dictionary<int, List<Warrior>> _squads;
    private static Dictionary<GameObject, Warrior> _warriorsPositions;
    private List<Warrior> m_EnemySquad;
    private Warrior m_CurrentWarrior;
    
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
        _warriorsPositions = new Dictionary<GameObject, Warrior>();
        m_Width = 8;
        m_Height = 8;
        
        DrawCells();
        InitEnemySquad();
    }

    public void Start()
    {
        _squads = SquadsManager.Squads;
        m_CurrentWarrior = _squads[SquadsManager.CurrentSquad][0];
        InitWarriorsOnField();
    }

    public void OnCellCLick()
    {
        var selectedCell = EventSystem.current.currentSelectedGameObject;
        // set active targetCell if currentCell exists and not doubleclicked
        if (m_CurrentCell != null && m_CurrentCell != selectedCell)
        {
            m_TargetCell = selectedCell;
        }
        else
        {
            // if currentCell doubleclicked, it will reset current cell and target cells
            if (m_CurrentCell == selectedCell)
            {
                m_CurrentCell = null;
                m_TargetCell = null;
            }
            else
            {
                if (_warriorsPositions.Keys.Contains(selectedCell))
                {
                    m_CurrentWarrior = _warriorsPositions[selectedCell];
                    m_CurrentCell = selectedCell;
                }
            }
        }

        UpdateCells();
        UpdateCurrentWarriorCard(m_CurrentWarrior);
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
    
    private void UpdateCurrentWarriorCard(Warrior currentWarrior)
    {
        currentWarriorCard.transform.Find("Icon").GetComponent<Image>().sprite = currentWarrior.Sprite;
        currentWarriorCard.transform.Find("Health").GetChild(0).GetComponent<Text>().text = currentWarrior.Health.ToString();
        currentWarriorCard.transform.Find("Armor").GetChild(0).GetComponent<Text>().text = currentWarrior.Armor.ToString();
    }
    
    private void UpdateCells()
    {
        foreach (var cell in m_CellsGrid)
        {
            cell.transform.Find("OnActive").gameObject.SetActive(cell == m_CurrentCell);
            cell.transform.Find("OnTarget").gameObject.SetActive(cell == m_TargetCell);
        }
    }

    private void InitWarriorsOnField()
    {
        var exitPosition = new Point(0, 7);
        var allyPositions = GetRandomPointsArray(3, new Point(0,3), new Point(4, 7), new List<Point>{exitPosition});
        var enemyPositions = GetRandomPointsArray(3, new Point(3,0), new Point(7, 4), allyPositions.Values.Append(exitPosition).ToList());

        m_ExitCell = m_CellsGrid[exitPosition.X, exitPosition.Y].transform.Find("Exit").gameObject;
        m_ExitCell.SetActive(true);

        for (var i=0; i<_squads[SquadsManager.CurrentSquad].Count; i++)
        {
            var allyPrefab = warriorPrefab;
            allyPrefab.transform.GetChild(0).GetComponent<Image>().sprite = _squads[SquadsManager.CurrentSquad][i].Sprite;
            _warriorsPositions.Add(m_CellsGrid[allyPositions[i].X, allyPositions[i].Y], _squads[SquadsManager.CurrentSquad][i]);
            SpawnWarrior(allyPrefab, allyPositions[i]);
            
            var enemyPrefab = warriorPrefab;
            enemyPrefab.transform.GetChild(0).GetComponent<Image>().sprite = m_EnemySquad[i].Sprite;
            _warriorsPositions.Add(m_CellsGrid[enemyPositions[i].X, enemyPositions[i].Y], m_EnemySquad[i]);
            SpawnWarrior(enemyPrefab, enemyPositions[i]);
        }
    }

    private void SpawnWarrior(GameObject prefab, Point position)
    {
        var warrior = Instantiate(prefab,
            m_CellsGrid[position.X, position.Y].transform.position, Quaternion.identity);
        warrior.transform.SetParent(m_CellsGrid[position.X, position.Y].transform);
    }
    
    public void OnMoveButton()
    {
        var warrior = m_CurrentCell.transform.Find("Warrior(Clone)").gameObject;
        
        _warriorsPositions[m_TargetCell] = m_CurrentWarrior;
        _warriorsPositions.Remove(m_CurrentCell);
        Move(warrior, m_TargetCell);

        m_CurrentCell = m_TargetCell;
        m_TargetCell = null;
        
        UpdateCells();
    }
    
    private void Move(GameObject warrior, GameObject targetCell)
    {
        warrior.transform.position = targetCell.transform.position;
        warrior.transform.parent = targetCell.transform;
    }
    
    private void InitEnemySquad()
    {
        m_EnemySquad = new List<Warrior>
        {
            new Warrior("Супер монстр", 7, 25, 24, 65, 12, 0.1, 0.7, SquadsManager.StaticWarriorIcons[3]),
            new Warrior("Чудовище", 2, 8, 10, 35, 6, 0.02, 0.7, SquadsManager.StaticWarriorIcons[4]),
            new Warrior("Мясо", 1, 5, 8, 20, 5, 0.1, 0.7, SquadsManager.StaticWarriorIcons[5])
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
