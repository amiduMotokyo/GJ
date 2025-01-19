using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class LevelSelectManager : MonoBehaviour
{
    public Sprite Level1;
    public Sprite Level2;
    public Sprite Level3;
    public Sprite Level4;
    public Sprite background_2;
    public Sprite background_3;
    public int SelectedLevel = 1;
    public void Refresh()
    {
        foreach (Transform child in this.transform)
        {
            if(child.name == "LevelImage")
            {
                switch (SelectedLevel)
                {
                    case 1:
                        child.GetComponent<Image>().sprite = Level1;
                        break;
                    case 2:
                        child.GetComponent<Image>().sprite = Level2;
                        break;
                    case 3:
                        child.GetComponent<Image>().sprite = Level3;
                        break;
                    case 4:
                        child.GetComponent<Image>().sprite = Level4;
                        break;

                }
                //string str = "UI/Title/Level_";  // 字符串部分
                //int num = SelectedLevel;         // 数字部分
                //child.GetComponent<Image>().sprite = Resources.Load<Sprite>(str + num);
            }
            if(child.name == "levelid")
            {
                child.GetChild(0).GetComponent<Text>().text = "关卡：" + SelectedLevel;
            }
        }
    }

}
