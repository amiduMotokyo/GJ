using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windforce : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _bubble;
    [Header("风压")]
    public float Windforce;
    [Header("方向")]
    public windDirection Direction;
    private Vector2 _winddir;

    private void Awake()
    {
        changedir(Direction);
    }



    private void Update()
    {
        addforce();
    }

    void changedir(windDirection dir)
    {
        switch (dir)
        {
            case windDirection.右: transform.localEulerAngles = new Vector3(0, 0, 0); _winddir = Vector2.right ; break;
            case windDirection.左: transform.localEulerAngles = new Vector3(0, 180, 0); _winddir = Vector2.left; break;
            case windDirection.下: transform.localEulerAngles = new Vector3(0, 0, -90); _winddir = Vector2.down; break;
            case windDirection.上: transform.localEulerAngles = new Vector3(0, 0, 90); _winddir = Vector2.up; break;
        }
    }

    void addforce()
    {
        if (_bubble != null)
        {
            for(var i=0; i < _bubble.Count;i++)
            {
                Debug.Log("move");
                var rb = _bubble[i].GetComponent<Rigidbody2D>();
                rb.AddForce(_winddir);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("bubble"))
        {
            Debug.Log("get");
            if (!_bubble.Contains(collision.gameObject))
            {
                _bubble.Add(collision.gameObject);
            }
            else
            {
                Debug.Log("exist");
            }
        }
    }
}

public enum windDirection
{
    上,
    下,
    左,
    右
}
