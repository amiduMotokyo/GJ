using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class LevelSelectManager : MonoBehaviour
{
    public int SelectedLevel = 1;
    public void Refresh()
    {
        foreach (Transform child in this.transform)
        {
            if(child.name == "LevelImage")
            {
                string str = "UI/Title/Level_";  // 字符串部分
                int num = SelectedLevel;         // 数字部分
                child.GetComponent<Image>().sprite = Resources.Load<Sprite>(str + num);
            }
            if(child.name == "levelid")
            {
                child.GetChild(0).GetComponent<Text>().text = "关卡：" + SelectedLevel;
            }
        }
    }

}
