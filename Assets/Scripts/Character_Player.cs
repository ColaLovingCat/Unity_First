using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : MonoBehaviour
{
    private CharacterController2D _chaControl;


    private bool _isJump = false;
    private bool _isCrouch = false;

    // À¿Õˆ±ﬂΩÁ
    public float deadLiney = -7.0f;

    // ∞¥º¸º‡≤‚
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
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");
        // Ω«…´µÙ¬‰£¨÷ÿ÷√”Œœ∑
        if (transform.position.y < deadLiney)
        {
            GameController.Instance.Restart();
        }
        //
        if (Input.GetButtonDown("Jump"))
            _isJump = true;
        if (Input.GetButtonDown("Crouch"))
            _isCrouch = true;
        else if (Input.GetButtonUp("Crouch"))
            _isCrouch = false;
    }

    void FixedUpdate()
    {
        _chaControl.Move(xRaw, _isCrouch, _isJump);
        _isJump = false;
    }

   
}
