using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BUTTONCT : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _animator.SetBool("ON", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _animator.SetBool("ON", false);
    }

    public void ClickStart()
    {
        StartCoroutine(PlayAnimationEnd_Start());
    }

    public void ClickChoose()
    {
        StartCoroutine(PlayAnimationEnd_Choose());
    }
    public void ClickExit()
    {
        StartCoroutine(PlayAnimationEnd_Exit());
    }
    IEnumerator PlayAnimationEnd_Start()
    {
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        GameObject.Find("GameRoot").GetComponent<LevelManager>().nowLevel = 1;
        GameRoot.Instance.SceneSystem.SetScene(new Level_1());
    }
    IEnumerator PlayAnimationEnd_Choose()
    {
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        foreach(Transform child in GameObject.Find("SelevtLevelPanel").transform)
        {
            //if (child.GetComponent<Image>())
            //    child.GetComponent<Image>().color = new Color(child.GetComponent<Image>().color.r, child.GetComponent<Image>().color.g, child.GetComponent<Image>().color.b, 255);
            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
            if (!canvasGroup)
            {
                canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1;
        }
    }
    IEnumerator PlayAnimationEnd_Exit()
    {
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        #if UNITY_EDITOR    //在编辑器模式下
                EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
