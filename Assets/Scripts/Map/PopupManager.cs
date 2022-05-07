using UnityEngine;
using UnityEngine.Serialization;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject onNewLevelPopup;
    [SerializeField] private GameObject squadPopup;
    [SerializeField] private GameObject levelsParent;

    public void CloseLevelPopup()
    {
        var currentSquad = SquadsManager.GetCurrentSquad();
        var squadsLocationBufer = SquadsManager.GetSquadsLocationBuffer();
        SquadsManager.MoveSquad(currentSquad, squadsLocationBufer);
        LevelManager.UpdateLevels(levelsParent);
        onNewLevelPopup.SetActive(false);
    }

    public void CloseSquadPopup()
    {
        squadPopup.SetActive(false);
    }

    public void Fight()
    {
        onNewLevelPopup.SetActive(false);
    }

    public void Search()
    {
        onNewLevelPopup.SetActive(false);
    }
}
