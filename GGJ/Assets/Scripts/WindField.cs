using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindField : MonoBehaviour
{
    public float force;             //��ѹ
    public windDirection Direction; //����
    public GameObject windPref;     //�糡Ԥ����
    
    //��������ò���
    private int _minX;
    private int _maxX;
    private int _minY;
    private int _maxY;

    private void Awake()
    {
        GenerateWind();
    }

    private void GenerateWind()
    {
        AreaCalculation();
        for(int i = _minX; i <= _maxX; i++)
        {
            for(int j = _minY; j <= _maxY; j++)
            {
                var wind = Instantiate(windPref, new Vector3(i, j, 0), transform.rotation);
                wind.GetComponent<windforce>().GetParameter(Direction, force);
            }
        }
    }

    private void AreaCalculation()
    {
        _minX = Mathf.CeilToInt(transform.position.x);
        _maxX = Mathf.FloorToInt(transform.position.x + transform.localScale.x) ;

        _minY = Mathf.CeilToInt(transform.position.y);
        _maxY = Mathf.FloorToInt(transform.position.y + transform.localScale.y);
    }
}
