using UnityEngine;

public class UnitUi : MonoBehaviour
{
    public void DisableHighlight()
    {
        transform.Find("Visual").Find("OnActiveBackground").gameObject.SetActive(false);
        transform.Find("Visual").Find("Bars Fill").gameObject.SetActive(false);
    }
    
    public void EnableHighlight()
    {
        transform.Find("Visual").Find("OnActiveBackground").gameObject.SetActive(true);
        transform.Find("Visual").Find("Bars Fill").gameObject.SetActive(true);
    }
}
