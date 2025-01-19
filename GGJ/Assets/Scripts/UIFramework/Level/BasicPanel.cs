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
            //���������˵���ѡ��
            SoundManager.Instance.PlaySound("���");
            Debug.Log("����˲˵�");
            PanelManager.Push(new MenuPanel());
        });
    }
}
