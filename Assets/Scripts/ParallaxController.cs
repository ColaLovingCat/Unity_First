using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    public Transform _camera; // 相机位置
    public float moveRate; // 背景随相机运动的比例
    private float cameraStartPoint; // 相机初始位置
    private Vector2 startPoint; // 背景初始位置

    public bool lockY = true; // 锁定Y方向的移动

    // Start is called before the first frame update
    void Start()
    {
        // 获取初始位置
        cameraStartPoint = _camera.position.x;
        startPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // float cameraBasic = _camera.position.x - cameraStartPoint;
        if (lockY)
        {
            transform.position = new Vector2(startPoint.x + _camera.position.x * moveRate, transform.position.y);
        }
        else
        {
            transform.position = new Vector2(startPoint.x + _camera.position.x * moveRate, startPoint.y + _camera.position.y * moveRate);
        }
    }
}
