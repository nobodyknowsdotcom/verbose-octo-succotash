using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public void LoadMap()
    {
        SceneManager.LoadScene("Map");
    }
    
    public void LoadBattle() 
    {
        SceneManager.LoadScene("Battle");
    }
}
