using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Поработать над классом
public class AreaOfEffectAbility : AttackingAbility
{
    public int area = 1;

    public override BattleInfo Use(BattleInfo info)
    {
        var targetPoint = GameObjectToPoint(info.targetCell);
        for (int i = targetPoint.X; i < targetPoint.X + area && i < 7; i++)
        {
            for (int j = targetPoint.Y; j < targetPoint.Y + area && j < 7; j++)
            {
                if (info.UnitsPositions.ContainsKey(info.CellsGrid[i, j]))
                {
                    info.targetCell = info.CellsGrid[i, j];
                    CalculateEnemyHealth(info);
                }
            }
        }

        return info;
    }
}
