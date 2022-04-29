using UnityEngine;

public class OnLevelClick : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    
    public void OnClick(int index)
    {
        var level = levelsParent.transform.GetChild(index).gameObject;
        LevelSelectionSystem.SetCurrentLevel(level);
    }
    
}
