using System;
using UnityEngine;

public class LevelSelectionSystem : MonoBehaviour
{
    
    [SerializeField] private GameObject levelsParent;
    private static int currentLevel;

    public void Update()
    {
        foreach (Transform level in levelsParent.transform)
        {
            if (level.name != currentLevel.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
            }
        }
    }

    public void Start()
    {
        var firstLevel = levelsParent.transform.GetChild(0).gameObject;
        SetCurrentLevel(firstLevel);
    }

    public static int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public static void SetCurrentLevel(GameObject level)
    {
        var enabledIcon = level.transform.Find("OnActive").gameObject;
        enabledIcon.SetActive(true);
        currentLevel = Int32.Parse(level.name);
        Debug.Log("Current level is " + currentLevel);
    }
}
