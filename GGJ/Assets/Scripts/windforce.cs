using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windforce : MonoBehaviour
{
    [SerializeField] private List<GameObject> _bubble;
    [SerializeField] private float Windforce;
    private Vector2 _winddir;

    private void Update()
    {
        addforce();
    }

    public void GetParameter(windDirection dir,float force)
    {
        Windforce = force;

        switch (dir)
        {
            case windDirection.Right: 

                transform.localEulerAngles = new Vector3(0, 0, 0); 
                _winddir = Vector2.right ; 
                break;

            case windDirection.Left: 

                transform.localEulerAngles = new Vector3(0, 180, 0); 
                _winddir = Vector2.left; 
                break;

            case windDirection.Down: 

                transform.localEulerAngles = new Vector3(0, 0, -90); 
                _winddir = Vector2.down; 
                break;

            case windDirection.Up: 

                transform.localEulerAngles = new Vector3(0, 0, 90); 
                _winddir = Vector2.up; 
                break;
        }
    }

    void addforce()
    {
        if (_bubble != null)
        {
            for(var i=0; i < _bubble.Count;i++)
            {
                //Debug.Log("Add force");
                if (_bubble[i] != null)
                {
                    var rb = _bubble[i].GetComponent<Rigidbody2D>();
                    rb.AddForce(_winddir * Windforce, ForceMode2D.Force);
                }
                else
                {
                    _bubble.Remove(_bubble[i]);
                }
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("bubble") || collision.CompareTag("player"))
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("bubble") || collision.CompareTag("player"))
        {
            Debug.Log("get");
            if (_bubble.Contains(collision.gameObject))
            {
                _bubble.Remove(collision.gameObject);
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
