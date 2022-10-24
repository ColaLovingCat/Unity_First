using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : MonoBehaviour
{
    // ��ɫͨ�ýű�
    private CharacterController2D _chaControl;

    // �����߽�
    public float deadLiney = -7.0f;

    // Start is called before the first frame update
    void Start()
    {
        _chaControl = GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // ��ɫ���䣬������Ϸ
        if (transform.position.y < deadLiney)
        {
            GetComponent<AudioSource>().enabled = false;
            GameController.Instance.Restart();
        }

    }

    void FixedUpdate()
    {
    }

}
