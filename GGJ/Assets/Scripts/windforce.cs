using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windforce : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _bubble;
    [Header("��ѹ")]
    public float Windforce;
    [Header("����")]
    public windDirection Direction;


    void addforce()
    {
        if (_bubble != null)
        {
            for(var i=0; i < _bubble.Count;)
            {
                //_bubble[i].GetComponent<Rigidbody2D>().AddForce();
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("bubble"))
        {
            _bubble.Add(collision.gameObject);
        }
    }
}

public enum windDirection
{
    ��,
    ��,
    ��,
    ��
}
