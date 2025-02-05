﻿using System.Collections;
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
        UITool.GetOrAddComponentInChildren<Button>("Start").onClick.AddListener(() =>
        {
            Animator _animator = UITool.GetGameObject().GetComponent<Animator>();

            //处理点击“开始”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了Start");
            GameObject.Find("GameRoot").GetComponent<LevelManager>().nowLevel = 1;
            PanelManager.Pop();
            GameRoot.Instance.SceneSystem.SetScene(new Level_1());
        });
        UITool.GetOrAddComponentInChildren<Button>("Choose").onClick.AddListener(() =>
        {
            //处理点击“选关”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了Choose");
            PanelManager.Push(new SelevtLevelPanel());
        });
        UITool.GetOrAddComponentInChildren<Button>("Exit").onClick.AddListener(() =>
        {
            //处理点击“退出”选项
            SoundManager.Instance.PlaySound("点击");
            Debug.Log("点击了Exit");
            #if UNITY_EDITOR    //在编辑器模式下
                        EditorApplication.isPlaying = false;
            #else
                    Application.Quit();
            #endif
        });
    }
}
