using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using UnityEngine;

public class MovingAbility : UnitAbility
{
    public void Awake()
    {
        abilityType = AbilityType.Moving;
    }

    public override BattleInfo Use(BattleInfo info)
    {
        //GameObject unitAsGameObject = m_CurrentCell.transform.Find("Unit(Clone)").gameObject;
        // --- Заменено (временно)
        GameObject unitAsGameObject = info.currentCell.transform.GetChild(4).gameObject;
        List<Point> barriers = GetUnitsAsPoints(info, info.currentCell, info.targetCell);
        Point currentPosition = GameObjectToPoint(info.currentCell);
        Point targetPosition = GameObjectToPoint(info.targetCell);
        List<Point> path = Bts(barriers, currentPosition, targetPosition);

        if (range < path.Count)
        {
            Debug.Log("Слишком далеко, ты не можешь туда сходить!");
            return info;
        }

        if (!info.UnitsPositions.ContainsKey(info.targetCell) && info.targetCell != info.exitCell && !info.UnitsPositions[info.currentCell].inBattleInfo.IsMoved)
        {
            info.UnitsPositions[info.targetCell] = info.currentUnit;
            info.UnitsPositions.Remove(info.currentCell);
            
            StartCoroutine(MoveAndGetCells(info, unitAsGameObject, path));
            info.currentCell = info.targetCell;
            info.targetCell = null;
            info.UnitsPositions[info.currentCell].inBattleInfo.Moved();
        }
        else if (info.targetCell == info.exitCell)
        {
            info.allySquad.Remove(info.UnitsPositions[info.currentCell]);
            info.UnitsPositions.Remove(info.currentCell);
            
            StartCoroutine(MoveAndDestroy(info, unitAsGameObject, path));
            info.currentCell = null;
            info.currentUnit = null;
        }
        else
        {
            Debug.Log("Ты не можешь сходить туда!");
        }

        return info;
    }

    private IEnumerator MoveAndGetCells(BattleInfo info, GameObject unit, List<Point> path)
    {
        if (!info.currentUnit.inBattleInfo.IsUsedAbility)
            info.reachableCells = GetAvailableCells(info, info.targetCell, info.currentUnit.abilities.primaryAttackSkill.range);
        
        foreach (var point in path)
        {
            unit.transform.position = info.CellsGrid[point.X, point.Y].transform.position;
            unit.transform.parent = info.CellsGrid[point.X, point.Y].transform;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator MoveAndDestroy(BattleInfo info, GameObject unit, List<Point> path)
    {
        foreach (var point in path)
        {
            unit.transform.position = info.CellsGrid[point.X, point.Y].transform.position;
            unit.transform.parent = info.CellsGrid[point.X, point.Y].transform;
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(unit);
    }

    private List<GameObject> GetAvailableCells(BattleInfo info, GameObject start, int range)
    {
        var result = new List<GameObject>();
        var barriers = GetUnitsAsPoints(info);

        foreach (var cell in info.CellsGrid)
        {
            if (barriers.Contains(GameObjectToPoint(cell))) continue;

            var currentPosition = GameObjectToPoint(start);
            var targetPosition = GameObjectToPoint(cell);
            var path = Bts(barriers, currentPosition, targetPosition);

            if (path.Count <= range)
            {
                foreach (var point in path)
                {
                    var cellAtPoint = info.CellsGrid[point.X, point.Y];
                    result.Add(cellAtPoint);
                }
            }
        }

        return result;
    }
}
