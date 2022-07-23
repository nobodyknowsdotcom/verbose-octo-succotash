using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class BattleInfo : MonoBehaviour
{
    // Было статическим
    public Dictionary<GameObject, BattleUnit> UnitsPositions;
    public List<BattleUnit> allySquad;
    public List<BattleUnit> enemySquad;
    public BattleUnit currentUnit;

    public GameObject[,] CellsGrid;
    public List<GameObject> availableCells;
    public List<GameObject> reachableCells;
    public GameObject currentCell;
    public GameObject targetCell;
    public List<Obstacle> obstacles;
    public GameObject exitCell;

    public List<Point> GetBarriers()
    {
        var result = new List<Point>();
        foreach (var obstacle in obstacles) result.Add(obstacle.Position);
        return result;
    }
}
