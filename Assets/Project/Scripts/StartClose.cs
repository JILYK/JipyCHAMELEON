using UnityEngine;

public class StartClose : MonoBehaviour
{
    public GameObject LogoStatic;
    public GameObject LogoAnimate;
    public void Closes()
    {
        LogoStatic.SetActive(false);
        LogoAnimate.SetActive(true);
        gameObject.SetActive(false);
     //   GameManager.Instance.ToggleWindowObjects(0);
    }
}