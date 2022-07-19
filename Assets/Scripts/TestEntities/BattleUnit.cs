using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    // ����� ���������� � �����
    public GeneralInfo generalInfo;
    public UnitStats stats;

    // ������ �����������
    public UnitAbilities abilities;

    // ���������� ��� ���
    public UnitBattleInfo inBattleInfo;

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
    }

    // ��������?
    //public BattleUnit GetCopy(bool isUnitAlly)
    //{
    //    var copy = new BattleUnit();
    //    copy.generalInfo = generalInfo;
    //    copy.stats = stats;
    //    copy.abilities = abilities;

    //    copy.inBattleInfo = inBattleInfo;
    //    copy.inBattleInfo.IsAlly = isUnitAlly;

    //    return copy;
    //}
}
