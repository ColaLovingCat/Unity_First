using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Eagle : EnemyController
{
    private Rigidbody2D _rigidbody2D;
    private Collider2D _collider2D;

    public float moveSpeed = 2f;
    private bool _flyUp;

    public Transform topPoint, bottomPoint;
    private float _topLimit, _bottomLimit;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<CircleCollider2D>();
        // 获取上下移动的边界位置
        transform.DetachChildren();
        _topLimit = topPoint.transform.position.y;
        _bottomLimit = bottomPoint.transform.position.y;
        Destroy(topPoint.gameObject);
        Destroy(bottomPoint.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        AutoMovement();
    }

    void AutoMovement()
    {
        if (_flyUp)
        {
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, moveSpeed);
            if (_rigidbody2D.position.y >= _topLimit)
            {
                Flip();
            }
        }
        else
        {
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, -moveSpeed);
            if (_rigidbody2D.position.y <= _bottomLimit)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        _flyUp = !_flyUp;
    }
}
