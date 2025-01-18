using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理全局的一些东西
/// </summary>
public class GameRoot : MonoBehaviour
{
    ///// <summary>
    ///// 确认窗口的状态，0为无效，1为确定，2为取消
    ///// </summary>
    //public int ComfirmStatus = 0;
    public static GameRoot Instance { get; private set; }
    /// <summary>
    /// 场景管理器
    /// </summary>
    public SceneSystem SceneSystem { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        SceneSystem = new SceneSystem();
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        SceneSystem.SetScene(new StartScene());
    }
}
