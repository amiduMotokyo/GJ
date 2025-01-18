using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StartManager : MonoBehaviour
{
    PanelManager panelManager;
    SelectionPanel selectionPanel = new SelectionPanel();
    private void Awake()
    {
        panelManager = new PanelManager();
    }
    // Start is called before the first frame update
    void Start()
    {
        panelManager.Push(selectionPanel);
    }
    private void Update()
    {
        //getKeyDownCode();
    }

}
