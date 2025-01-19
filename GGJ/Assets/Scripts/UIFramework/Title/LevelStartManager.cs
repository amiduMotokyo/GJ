using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelStartManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite background_0;
    public Sprite background_1;
    public Sprite background_2;
    public Sprite background_3;
    public Sprite background_4;
    // Start is called before the first frame update
    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Transform child in GameObject.Find("SelevtLevelPanel").transform)
        {
            if (child.name == "background")
            {
                child.GetComponent<Image>().sprite = background_1;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Transform child in GameObject.Find("SelevtLevelPanel").transform)
        {
            if (child.name == "background")
            {
                child.GetComponent<Image>().sprite = background_0;
            }
        }
    }
}
