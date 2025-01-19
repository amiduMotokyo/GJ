using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipedoor : MonoBehaviour
{
    [SerializeField]private Animator _ani;

    void Start()
    {
        _ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OFF()
    {
        _ani.SetBool("OFF", true);
        //transform.GetComponent<BoxCollider2D>().enabled = false;
    }
}
