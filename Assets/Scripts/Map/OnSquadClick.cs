using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnSquadClick : MonoBehaviour
{
    [SerializeField] private GameObject squadsParent;
    
    public void Start()
    {
        var defaultSquad = squadsParent.transform.GetChild(1).gameObject;
        Map.SetCurrentSquad(defaultSquad);
    }
    
    public void OnClick()
    {
        var squad = EventSystem.current.currentSelectedGameObject;
        Map.SetCurrentSquad(squad);
        
        foreach (Transform e in squadsParent.transform)
        {
            if (Int32.Parse(e.name) == Map.GetCurrentSquad())
            {
                var enabledIcon = e.transform.Find("OnActive").gameObject;
                enabledIcon.SetActive(true);
            }
            else
            {
                var enabledIcon = e.transform.Find("OnActive").gameObject;
                enabledIcon.SetActive(false);
            }
        }
    }
}
