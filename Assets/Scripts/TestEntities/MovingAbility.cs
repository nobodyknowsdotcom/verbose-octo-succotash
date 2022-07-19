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
        // --- �������� (��������)
        GameObject unitAsGameObject = info.m_CurrentCell.transform.GetChild(4).gameObject;
        List<Point> barriers = GetUnitsAsPoints(info, info.m_CurrentCell, info.m_TargetCell);
        Point currentPosition = GameObjectToPoint(info.m_CurrentCell);
        Point targetPosition = GameObjectToPoint(info.m_TargetCell);
        List<Point> path = Bts(barriers, currentPosition, targetPosition);

        if (range < path.Count)
        {
            Debug.Log("������� ������, �� �� ������ ���� �������!");
            return info;
        }

        if (!info._unitsPositions.ContainsKey(info.m_TargetCell) && info.m_TargetCell != info.m_ExitCell && !info._unitsPositions[info.m_CurrentCell].inBattleInfo.IsMoved)
        {
            info._unitsPositions[info.m_TargetCell] = info.m_CurrentUnit;
            info._unitsPositions.Remove(info.m_CurrentCell);

            info.m_CurrentCell = info.m_TargetCell;
            info.m_TargetCell = null;

            StartCoroutine(Move(info, unitAsGameObject, path));
            info._unitsPositions[info.m_CurrentCell].inBattleInfo.Moved();
        }
        else if (info.m_TargetCell == info.m_ExitCell)
        {
            info.m_AllySquad.Remove(info._unitsPositions[info.m_CurrentCell]);
            info._unitsPositions.Remove(info.m_CurrentCell);

            StartCoroutine(Move(info, unitAsGameObject, path));
            Destroy(unitAsGameObject);

            //if (m_AllySquad.Count != 0)
            //{
            //    SwitchToNextUnit();
            //}
        }
        else
        {
            Debug.Log("�� �� ������ ������� ����!");
        }

        return info;
    }

    private IEnumerator Move(BattleInfo info, GameObject unit, List<Point> path)
    {
        foreach (var point in path)
        {
            unit.transform.position = info.m_CellsGrid[point.X, point.Y].transform.position;
            unit.transform.parent = info.m_CellsGrid[point.X, point.Y].transform;
            yield return new WaitForSeconds(0.15f);
        }

        info.m_ReachableCells = GetAvailableCells(info, info.m_CurrentCell, range);
    }

    private List<GameObject> GetAvailableCells(BattleInfo info, GameObject start, int range)
    {
        var result = new List<GameObject>();
        var barriers = GetUnitsAsPoints(info);

        foreach (var cell in info.m_CellsGrid)
        {
            if (barriers.Contains(GameObjectToPoint(cell))) continue;

            var currentPosition = GameObjectToPoint(start);
            var targetPosition = GameObjectToPoint(cell);
            var path = Bts(barriers, currentPosition, targetPosition);

            if (path.Count <= range)
            {
                foreach (var point in path)
                {
                    var cellAtPoint = info.m_CellsGrid[point.X, point.Y];
                    result.Add(cellAtPoint);
                }
            }
        }

        return result;
    }
}
