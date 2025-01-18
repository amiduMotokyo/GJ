using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// 开始场景
/// </summary>
public class StartScene : SceneState
{
    readonly string sceneName = "Title";
    PanelManager panelManager;

    public override void OnEnter()
    {
        panelManager = new PanelManager();
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
            SceneManager.sceneLoaded += SceneLoaded;
        }
        else
        {
            panelManager.Push(new SelectionPanel());
        }
    }

    public override void OnExit()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        panelManager.PopAll();
    }
    /// <summary>
    /// 场景加载完毕后执行的方法
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="load"></param>
    private void SceneLoaded(Scene scene, LoadSceneMode load)
    {
        panelManager.Push(new SelectionPanel());
        Debug.Log($"{sceneName}场景加载完毕");
    }
}
