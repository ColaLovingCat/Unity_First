using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform _camera; // ���λ��
    public float moveRate; // ����������˶��ı���
    private float cameraStartPoint; // �����ʼλ��
    private float startPoint; // ������ʼλ��

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
