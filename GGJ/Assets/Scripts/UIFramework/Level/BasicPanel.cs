using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BasicPanel : BasePanel
{
    static readonly string path = "Prefab/Title/BasicPanel";
    public BasicPanel() : base(new UIType(path)) { }

    public override void OnEnter()
    {
        SoundManager.Instance.PlayBgm("BGM");
        UITool.GetOrAddComponentInChildren<Button>("Menu").onClick.AddListener(() =>
        {
            //处理点击“菜单”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了菜单");
            PanelManager.Push(new MenuPanel());
        });
    }
}
