using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ARROWCT : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator _ANI;

    private void Awake()
    {
        _ANI = GetComponent<Animator>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _ANI.SetBool("on", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _ANI.SetBool("on", false);
    }
}
