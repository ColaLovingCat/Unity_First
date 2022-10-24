using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform _camera; // 相机位置
    public float moveRate; // 背景随相机运动的比例
    private float cameraStartPoint; // 相机初始位置
    private float startPoint; // 背景初始位置

    // Start is called before the first frame update
    void Start()
    {
        cameraStartPoint = _camera.position.x;
        startPoint = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        float cameraBasic = _camera.position.x - cameraStartPoint;
        transform.position = new Vector2(startPoint + _camera.position.x * moveRate, transform.position.y);
    }
}
