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
        int totalHealing = (int)(info.currentUnit.stats.MaxHealth * healingAmount);
        info.currentUnit.stats.Health += totalHealing;
        if (info.currentUnit.stats.Health > info.currentUnit.stats.MaxHealth)
        {
            info.currentUnit.stats.Health = info.currentUnit.stats.MaxHealth;
        }

        info.currentUnit.inBattleInfo.IsUsedAbility = true;

        return info;
    }
}
