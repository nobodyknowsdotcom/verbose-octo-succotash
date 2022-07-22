using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInfo : MonoBehaviour
{
    // Было статическим
    public Dictionary<GameObject, BattleUnit> _unitsPositions;
    public List<BattleUnit> m_AllySquad;
    public List<BattleUnit> m_EnemySquad;
    public BattleUnit m_CurrentUnit;

    public GameObject[,] m_CellsGrid;
    public List<GameObject> m_AvailableCells;
    public List<GameObject> m_ReachableCells;
    public GameObject m_CurrentCell;
    public GameObject m_TargetCell;
    public GameObject m_ExitCell;
}
