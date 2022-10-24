using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : MonoBehaviour
{
    // 角色通用脚本
    private CharacterController2D _chaControl;

    // 死亡边界
    public float deadLiney = -7.0f;

    // Start is called before the first frame update
    void Start()
    {
        _chaControl = GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // 角色掉落，重置游戏
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
