using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquadsManager : MonoBehaviour
{
    [SerializeField] private GameObject squadsParent;
    
    public void Start()
    {
        var defaultSquad = squadsParent.transform.GetChild(0).gameObject;
        Map.SetCurrentSquad(defaultSquad);
        UpdateSquadsPanel(squadsParent, Map.GetCurrentSquad());
    }
    
    public void OnClick()
    {
        var squad = EventSystem.current.currentSelectedGameObject;
        Map.SetCurrentSquad(squad);
    }

    private void UpdateSquadsPanel(GameObject squads, int currentSquad)
    {
        foreach (Transform e in squads.transform)
        {
            if (Int32.Parse(e.name) == currentSquad)
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
