using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SelevtLevelPanel : BasePanel
{
    static readonly string path = "Prefab/Title/SelevtLevelPanel";
    public SelevtLevelPanel() : base(new UIType(path)) { }

    public override void OnEnter()
    {
        UITool.GetOrAddComponentInChildren<Button>("start").onClick.AddListener(() =>
        {
            //处理点击“开始游戏”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了开始游戏");
            GameObject.Find("GameRoot").GetComponent<LevelManager>().nowLevel = UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel;
            PanelManager.Pop();
            switch (UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel)
            {
                case 1:
                    GameRoot.Instance.SceneSystem.SetScene(new Level_1());
                    return;
                case 2:
                    GameRoot.Instance.SceneSystem.SetScene(new Level_2());
                    return;
                case 3:
                    GameRoot.Instance.SceneSystem.SetScene(new Level_3());
                    return;
                case 4:
                    GameRoot.Instance.SceneSystem.SetScene(new Level_4());
                    return;
                default:
                    throw new KeyNotFoundException("No class found for the given key.");
            }
        });
        UITool.GetOrAddComponentInChildren<Button>("right").onClick.AddListener(() =>
        {
            //处理点击“右箭头”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了右箭头");
            if(UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel < 4)
            {
                UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel++;
            }
            UITool.GetGameObject().GetComponent<LevelSelectManager>().Refresh();
        });
        UITool.GetOrAddComponentInChildren<Button>("left").onClick.AddListener(() =>
        {
            //处理点击“左箭头”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了左箭头");
            if (UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel > 1)
            {
                UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel--;
            }
            UITool.GetGameObject().GetComponent<LevelSelectManager>().Refresh();
        });
        UITool.GetOrAddComponentInChildren<Button>("return").onClick.AddListener(() =>
        {
            //处理点击“返回”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("选择了返回选项");
            PanelManager.Pop();
        });
    }
    public override void OnExit()
    {
        UIManager.DestroyUI(UIType, true);
    }
}
