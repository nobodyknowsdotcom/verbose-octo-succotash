using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingAbility : UnitAbility
{
    public double damageMultiplier = 1;
    public int shotsCount = 1;

    public void Awake()
    {
        abilityType = AbilityType.Attacking;
    }

    public override BattleInfo Use(BattleInfo info)
    {
        CalculateEnemyHealth(info);
        info.m_TargetCell.GetComponent<TileVisualController>().HighlightTile();
        return info;
    }

    public void CalculateEnemyHealth(BattleInfo info)
    {
        // ����������
        // --- double coofDamage
        // --- int armCoofDamage
        // --- double coofAccuracy
        // --- int count

        // ���������
        var enemy = info._unitsPositions[info.m_TargetCell];

        // ����������
        int damage = 0;
        var random = new System.Random();

        // i - ���������� ���������
        for (int i = 0; i < shotsCount; i++)
        {
            // +0 - ��� +coofAccuracy
            if (random.NextDouble() <= (info.m_CurrentUnit.stats.Accuracy + 0) * (1 - enemy.stats.DodgeChance))
                // 1 - coofDamage
                damage += (int)(info.m_CurrentUnit.stats.Damage * damageMultiplier);
        }

        // 1 - armCoofDamage
        if (damage * 1 > enemy.stats.Armor)
        {
            damage = (damage * 1 - enemy.stats.Armor) / 1;
            enemy.stats.Armor = 0;
        }
        else
        {
            enemy.stats.Armor -= damage * 1;
            damage = 0;
        }

        if (enemy.stats.Health - damage > 0)
        {
            enemy.stats.Health -= damage;
        }
        else
        {
            enemy.stats.Health = 0;
        }

        info.m_CurrentUnit.inBattleInfo.IsUsedAbility = true;
    }
}
