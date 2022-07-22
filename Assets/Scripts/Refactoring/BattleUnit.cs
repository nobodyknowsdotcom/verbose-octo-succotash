using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    // Блоки информации о юните
    public GeneralInfo generalInfo;
    public UnitStats stats;

    // Боевые способности
    public UnitAbilities abilities;

    // Информация для боя
    public UnitBattleInfo inBattleInfo;

    // Контроллер для визуала юнита
    public UnitUi uiController;
    public BattleUnit(BattleUnit unitToCopyFrom)
    {
        generalInfo = unitToCopyFrom.generalInfo;
        stats = unitToCopyFrom.stats;
        abilities = unitToCopyFrom.abilities;
        inBattleInfo = unitToCopyFrom.inBattleInfo;
    }

    public void Awake()
    {
        if (generalInfo == null)
        {
            generalInfo = gameObject.GetComponent<GeneralInfo>();
        }

        if (stats == null)
        {
            stats = gameObject.GetComponent<UnitStats>();
        }

        if (abilities == null)
        {
            abilities = gameObject.GetComponentInChildren<UnitAbilities>();
        }
        
        if (uiController == null)
        {
            uiController = gameObject.GetComponentInChildren<UnitUi>();
        }
    }
}
