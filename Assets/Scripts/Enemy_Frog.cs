using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Frog : EnemyController
{

    private Rigidbody2D _rigidbody2D;
    private Collider2D _collider2D;

    public float runSpeed = 5f;
    public float jumpForce = 5f;
    public LayerMask ground;
    private bool _isGrounded;

    public float stayTime = 3;
    private bool _facingRight;

    public Transform leftPoint, rightPoint;
    private float _leftLimit, _rightLimit;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<CircleCollider2D>();
        // 获取左右移动的边界位置
        transform.DetachChildren();
        _leftLimit = leftPoint.transform.position.x;
        _rightLimit = rightPoint.transform.position.x;
        Destroy(leftPoint.gameObject);
        Destroy(rightPoint.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        _isGrounded = false;
        if (_collider2D.IsTouchingLayers(ground))
        {
            _isGrounded = true;
        }
        // 由Animator Event驱动
        //AutoMovement();
        SwitchAnimator();
    }

    public void AutoMovement()
    {
        if (transform.position.x < _leftLimit)
        {
            Flip();
        }
        if (transform.position.x >= _rightLimit)
        {
            Flip();
        }
        //
        if (!_facingRight)
        {

            if (_isGrounded)
            {
                _animator.SetBool("isJump", true);
                _rigidbody2D.velocity = new Vector2(-runSpeed, jumpForce);
            }
        }
        else
        {
            if (_isGrounded)
            {
                _animator.SetBool("isJump", true);
                _rigidbody2D.velocity = new Vector2(runSpeed, jumpForce);
            }
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        _facingRight = !_facingRight;
        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void SwitchAnimator()
    {
        if (_animator.GetBool("isJump"))
        {
            if (_rigidbody2D.velocity.y < 1)
            {
                _animator.SetBool("isJump", false);
                _animator.SetBool("isFall", true);
            }
        }
        if (_isGrounded && _animator.GetBool("isFall"))
        {
            _animator.SetBool("isFall", false);
        }
    }

}
