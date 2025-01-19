using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    private Animator _ani;
    public GameObject pipedoor;
    private bool _triggered;
    private int _playerLevel;

    void Start()
    {
        _ani = GetComponent<Animator>();
    }

    void Update()
    {
    }

    private void pushdown()
    {
        _ani.SetBool("ON", true);
        pipedoor.GetComponent<Pipedoor>().OFF();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if ( !_triggered && collision.CompareTag("player"))
        {
            Debug.Log("tri");
            pushdown();
            _triggered = true;
        }
    }
}
