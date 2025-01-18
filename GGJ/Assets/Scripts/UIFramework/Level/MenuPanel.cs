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
            //处理点击“continue”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了continue");
            PanelManager.Pop();
        });
        UITool.GetOrAddComponentInChildren<Button>("back").onClick.AddListener(() =>
        {
            //处理点击“back”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了back");
            GameRoot.Instance.SceneSystem.SetScene(new StartScene());
        });
    }
    public override void OnExit()
    {
        UIManager.DestroyUI(UIType, true);
    }
}
