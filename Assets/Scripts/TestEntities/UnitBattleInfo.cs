using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBattleInfo : MonoBehaviour
{
    public bool IsAlly;
    public bool IsMoved = false;
    public bool IsUsedAbility = false;

    public void Moved()
    {
        IsMoved = true;
    }

    public void RefreshAbilities()
    {
        IsUsedAbility = false;
    }

    public void RefreshMoving()
    {
        IsMoved = false;
    }

    public void RefreshAbilitiesAndMoving()
    {
        RefreshAbilities();
        RefreshMoving();
    }
}