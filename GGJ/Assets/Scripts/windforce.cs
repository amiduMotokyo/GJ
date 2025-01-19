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
            case windDirection.Right: transform.localEulerAngles = new Vector3(0, 0, 0); _winddir = Vector2.right ; break;
            case windDirection.Left: transform.localEulerAngles = new Vector3(0, 180, 0); _winddir = Vector2.left; break;
            case windDirection.Down: transform.localEulerAngles = new Vector3(0, 0, -90); _winddir = Vector2.down; break;
            case windDirection.Up: transform.localEulerAngles = new Vector3(0, 0, 90); _winddir = Vector2.up; break;
        }
    }

    void addforce()
    {
        if (_bubble != null)
        {
            for(var i=0; i < _bubble.Count;i++)
            {
                Debug.Log("Add force");
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
    Up,
    Down,
    Left,
    Right
}
