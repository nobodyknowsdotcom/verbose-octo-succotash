using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popup;

    public void ClosePopup()
    {
        popup.SetActive(false);
    }
}
