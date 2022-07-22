using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingAbility : UnitAbility
{
    public double healingAmount;

    public void Awake()
    {
        abilityType = AbilityType.Healing;
    }

    public override BattleInfo Use(BattleInfo info)
    {
        // Базовая способность лечения
        // Лечит себя на процент от максимального здоровья
        int totalHealing = (int)(info.m_CurrentUnit.stats.MaxHealth * healingAmount);
        info.m_CurrentUnit.stats.Health += totalHealing;
        if (info.m_CurrentUnit.stats.Health > info.m_CurrentUnit.stats.MaxHealth)
        {
            info.m_CurrentUnit.stats.Health = info.m_CurrentUnit.stats.MaxHealth;
        }

        info.m_CurrentUnit.inBattleInfo.IsUsedAbility = true;

        return info;
    }
}
