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
            //����������ʼ��Ϸ��ѡ��
            SoundManager.Instance.PlaySound("���");
            Debug.Log("����˿�ʼ��Ϸ");
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
            //���������Ҽ�ͷ��ѡ��
            SoundManager.Instance.PlaySound("���");
            Debug.Log("������Ҽ�ͷ");
            if(UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel < 4)
            {
                UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel++;
            }
            UITool.GetGameObject().GetComponent<LevelSelectManager>().Refresh();
        });
        UITool.GetOrAddComponentInChildren<Button>("left").onClick.AddListener(() =>
        {
            //�����������ͷ��ѡ��
            SoundManager.Instance.PlaySound("���");
            Debug.Log("��������ͷ");
            if (UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel > 1)
            {
                UITool.GetGameObject().GetComponent<LevelSelectManager>().SelectedLevel--;
            }
            UITool.GetGameObject().GetComponent<LevelSelectManager>().Refresh();
        });
        UITool.GetOrAddComponentInChildren<Button>("return").onClick.AddListener(() =>
        {
            //�����������ء�ѡ��
            SoundManager.Instance.PlaySound("���");
            Debug.Log("ѡ���˷���ѡ��");
            PanelManager.Pop();
        });
    }
    public override void OnExit()
    {
        UIManager.DestroyUI(UIType, true);
    }
}
