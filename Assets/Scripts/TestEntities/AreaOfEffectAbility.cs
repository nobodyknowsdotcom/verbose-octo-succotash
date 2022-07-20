using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���������� ��� �������
public class AreaOfEffectAbility : AttackingAbility
{
    public int area = 1;

    public override BattleInfo Use(BattleInfo info)
    {
        var targetPoint = GameObjectToPoint(info.m_TargetCell);
        for (int i = targetPoint.X; i < targetPoint.X + area && i < 8; i++)
        {
            for (int j = targetPoint.Y; j < targetPoint.Y + area && j < 8; j++)
            {
                info.m_CellsGrid[i, j].GetComponent<TileVisualController>().HighlightTile();
                if (info._unitsPositions.ContainsKey(info.m_CellsGrid[i, j]))
                {
                    info.m_TargetCell = info.m_CellsGrid[i, j];
                    CalculateEnemyHealth(info);
                }
            }
        }

        return info;
    }
}