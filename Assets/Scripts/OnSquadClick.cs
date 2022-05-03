using UnityEngine;
using UnityEngine.EventSystems;

public class OnSquadClick : MonoBehaviour
{
    public void OnClick()
    {
        var squad = EventSystem.current.currentSelectedGameObject;
        LevelSelectionSystem.SetCurrentSquad(squad);
    }
}
