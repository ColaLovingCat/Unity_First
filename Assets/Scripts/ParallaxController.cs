using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    public Transform _camera; // ���λ��
    public float moveRate; // ����������˶��ı���
    private float cameraStartPoint; // �����ʼλ��
    private Vector2 startPoint; // ������ʼλ��

    public bool lockY = true; // ����Y������ƶ�

    // Start is called before the first frame update
    void Start()
    {
        // ��ȡ��ʼλ��
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
