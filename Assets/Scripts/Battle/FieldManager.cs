using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject cellsField;
    private GameObject gameGrid;
    private int width;
    private int height;
    private float cellSize = 1f;

    public void Awake()
    {
        width = 8;
        height = 8;
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

        var gameGrid = new GameObject[width, height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                gameGrid[i, j] = Instantiate(cellPrefab, new Vector3(i * cellSize, j * cellSize, 0), Quaternion.identity);
                gameGrid[i, j].transform.parent = cellsField.transform;
                gameGrid[i, j].gameObject.name = i + "_" + j;
            }
        }
    }
}
