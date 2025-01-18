using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public void refresh()
    {
        foreach (Transform child in this.transform)
        {
            if (child.name == "level")
            {
                child.GetComponent<Text>().text = "ตฺ" + GameObject.Find("GameRoot").GetComponent<LevelManager>().nowLevel + "นุ";
            }
        }
    }
}
