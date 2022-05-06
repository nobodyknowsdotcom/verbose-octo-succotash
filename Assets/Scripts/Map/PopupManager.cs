using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject levelsParent;

    public void ClosePopup()
    {
        var currentSquad = Map.GetCurrentSquad();
        var squadsLocationBufer = SquadsManager.GetSquadsLocationBuffer();
        SquadsManager.MoveSquad(currentSquad, squadsLocationBufer);
        LevelManager.UpdateLevels(levelsParent);
        popup.SetActive(false);
    }

    public void Fight()
    {
        popup.SetActive(false);
    }

    public void Search()
    {
        popup.SetActive(false);
    }
}
