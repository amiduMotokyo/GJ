using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BUTTONCT : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _animator.SetBool("CLICK", true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _animator.SetBool("ON", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _animator.SetBool("ON", false);
    }
}
