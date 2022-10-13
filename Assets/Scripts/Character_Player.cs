using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : MonoBehaviour
{
    // 角色通用脚本
    private CharacterController2D _chaControl;

    // 角色状态
    private bool _isJump = false;
    private bool _isCrouch = false;

    // 死亡边界
    public float deadLiney = -7.0f;

    // 按键监测
    private float x, y;
    private float xRaw, yRaw;


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
            GameController.Instance.Restart();
        }
        // 获取按键
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");
        // 单机空格键跳跃
        if (Input.GetButtonDown("Jump"))
            _isJump = true;
        // 长按C键下蹲
        if (Input.GetButtonDown("Crouch"))
            _isCrouch = true;
        else if (Input.GetButtonUp("Crouch"))
            _isCrouch = false;
    }

    void FixedUpdate()
    {
        _chaControl.Move(xRaw, _isCrouch, _isJump);
        //
        _isJump = false;
    }

}
