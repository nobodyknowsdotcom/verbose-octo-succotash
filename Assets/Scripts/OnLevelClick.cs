using UnityEngine;
using UnityEngine.EventSystems;

public class OnLevelClick : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    
    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        LevelSelectionSystem.SetCurrentLevel(level);
    }
    
}
