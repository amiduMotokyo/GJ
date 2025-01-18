using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : BasePanel
{
    static readonly string path = "Prefab/Title/MenuPanel";
    public MenuPanel() : base(new UIType(path)) { }

    public override void OnEnter()
    {
        UITool.GetGameObject().GetComponent<MenuManager>().refresh();
        UITool.GetOrAddComponentInChildren<Button>("continue").onClick.AddListener(() =>
        {
            //��������continue��ѡ��
            SoundManager.Instance.PlaySound("���");
            Debug.Log("�����continue");
            PanelManager.Pop();
        });
        UITool.GetOrAddComponentInChildren<Button>("back").onClick.AddListener(() =>
        {
            //��������back��ѡ��
            SoundManager.Instance.PlaySound("���");
            Debug.Log("�����back");
            GameRoot.Instance.SceneSystem.SetScene(new StartScene());
        });
    }
    public override void OnExit()
    {
        UIManager.DestroyUI(UIType, true);
    }
}
