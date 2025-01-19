using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SelectionPanel : BasePanel
{
    static readonly string path = "Prefab/Title/SelectionPanel";
    public SelectionPanel() : base(new UIType(path)) { }
    public override void OnEnter()
    {
        SoundManager.Instance.PlayBgm("BGM");
        UITool.GetOrAddComponentInChildren<Button>("Start").onClick.AddListener(() =>
        {
            //处理点击“开始”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了Start");
            Animator _animator = UITool.GetOrAddComponentInChildren<Animator>("Start");
            _animator.SetBool("CLICK", true);
            UITool.GetOrAddComponentInChildren<BUTTONCT>("Start").ClickStart();
        });
        UITool.GetOrAddComponentInChildren<Button>("Choose").onClick.AddListener(() =>
        {
            //处理点击“选关”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了Choose");
            Animator _animator = UITool.GetOrAddComponentInChildren<Animator>("Choose");
            _animator.SetBool("CLICK", true);
            UITool.GetOrAddComponentInChildren<BUTTONCT>("Choose").ClickChoose();
            PanelManager.Push(new SelevtLevelPanel(),true);
        });
        UITool.GetOrAddComponentInChildren<Button>("Exit").onClick.AddListener(() =>
        {
            //处理点击“退出”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了Exit");
            Animator _animator = UITool.GetOrAddComponentInChildren<Animator>("Exit");
            _animator.SetBool("CLICK", true);
            UITool.GetOrAddComponentInChildren<BUTTONCT>("Exit").ClickExit();
        });
    }
}
