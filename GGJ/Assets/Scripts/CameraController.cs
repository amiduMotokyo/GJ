using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private GameObject _cameraBound;
    [SerializeField] private CinemachineConfiner2D _confiner;

    private void Awake()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _confiner = GetComponent<CinemachineConfiner2D>();
    }

    private void Update()
    {
        if(_player == null)
        {
            REF();
        }
    }

    public void REF()
    {
        _player = GameObject.FindWithTag("player");
        _cameraBound = GameObject.Find("CameraBounds").gameObject;
        _confiner.m_BoundingShape2D = _cameraBound.GetComponent<PolygonCollider2D>();
        if (_player != null)
        {
            _virtualCamera.Follow = _player.transform;
        }
    }
}
