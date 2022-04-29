using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class LevelSelectionSystem : MonoBehaviour
{
    
    [SerializeField] public GameObject levelsParent;
    private static GameObject CurrentLevel;

    public void Start()
    {
        var firstLevel = levelsParent.transform.GetChild(4).gameObject;
        SetCurrentLevel(firstLevel);
    }

    public static GameObject GetCurrentLevel()
    {
        return CurrentLevel;
    }
    
    public static void SetCurrentLevel(GameObject level)
    {
        var enabledIcon = level.transform.Find("OnActive").gameObject;
        enabledIcon.SetActive(true);
        CurrentLevel = level;
    }
}
