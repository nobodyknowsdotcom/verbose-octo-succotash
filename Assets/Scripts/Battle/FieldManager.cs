using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldManager : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject cellsField;
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
}
