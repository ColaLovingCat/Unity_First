using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    // 
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    public AudioSource _jumpAudio;
    public Renderer _render;

    /** MoveConfig **/
    [HideInInspector][SerializeField] private float _running; // record moving status
    [HideInInspector][SerializeField] private float _movementSpeed = 6f; // moving speed
    [HideInInspector][Range(0, 0.3f)][SerializeField] private float _movementSmoothing = 0.05f; // How much to smooth out the movement

    // Jump Info
    [HideInInspector][Range(0, 8f)][SerializeField] private float _jumpForce = 8f; // Amount of force added when the player jumps.
    [HideInInspector][SerializeField] private bool _canAirControl = false; // 在空中是否可以转向
    [HideInInspector][SerializeField] private int _jumpDefaultCount = 2; // Whether or not a player can steer while jumping;
    [HideInInspector][SerializeField] private int _jumpCount; //
    private bool _isLanded; // Whether or not a player can steer while jumping;
    // Ground Info
    [SerializeField] private bool _isGrounded = false;
    [HideInInspector][SerializeField] private LayerMask _ground; // 用于判断地面的图层
    [HideInInspector][SerializeField] private Transform _groundCheck; // 地面判断点
    const float _groundCheckRadius = .2f; // 地面判断半径

    // Crouch Info
    private bool _isCrouch, _canStand; // Whether or not the player is crouched
    [HideInInspector][Range(0, 1)][SerializeField] private float _crouchSpeed = 0.36f; // 下蹲的移动速度
    // Ceiling Info
    [HideInInspector][SerializeField] private Collider2D _colliderCeilingDisable; // 头部碰撞体，下蹲时失效
    [HideInInspector][SerializeField] public Transform _ceilingCheck; // 头顶判断点
    const float _ceilingCheckRadius = .2f; // 头顶判断半径

    private Vector3 velocity = Vector3.zero;
    private bool _facingRight = true;  // For determining which way the player is currently facing.

    private bool _isHurt; // 角色受伤状态
    public float blinkTime;
    public int blinks;
    [HideInInspector][SerializeField] private float _hurtForce = 2f;

    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _render = GetComponent<Renderer>();

    }

    void FixedUpdate()
    {
        // 重置落地状态
        _isLanded = false;
        // 地面检测
        CheckLanded();
    }

    /// <summary>
    /// 暴露在外的移动方法
    /// </summary>
    /// <param name="move">左右移动方向</param>
    /// <param name="crouch">是否下蹲</param>
    /// <param name="jump">是否跳跃</param>
    public void Move(float move, bool crouch, bool jump)
    {
        // 没有受伤时才能操作
        if (!_isHurt)
        {
            Movement(move, crouch);
            Jump(jump);
        }
        //
        SwitchAnimation();
    }

    /// <summary>
    /// 移动脚本
    /// </summary>
    /// <param name="move"></param>
    /// <param name="crouch"></param>
    private void Movement(float move, bool crouch)
    {
        // 重置站立状态
        _canStand = true;
        // 头顶检测
        if (Physics2D.OverlapCircle(_ceilingCheck.position, _ceilingCheckRadius, _ground))
        {
            _canStand = false;
            if (!crouch)
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                crouch = true;
            }
        }
        _isCrouch = crouch;
        // 角色在地面上
        if (_isGrounded)
        {
            // If crouching
            if (crouch)
            {
                // 以下蹲的速度移动
                move *= _crouchSpeed;
                // 当下蹲时将头部的碰撞器失效
                if (_colliderCeilingDisable != null) _colliderCeilingDisable.enabled = false;
            }
            else
            {
                // 恢复头部碰撞器
                if (_colliderCeilingDisable != null) _colliderCeilingDisable.enabled = true;
            }
            //
            _running = move;
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * _movementSpeed, _rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, _movementSmoothing);
        }
        // 角色在地面上 或 空中允许 才可转向
        if (_isGrounded || _canAirControl)
        {
            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !_facingRight)
            {
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && _facingRight)
            {
                Flip();
            }
        }
    }

    /// <summary>
    /// 转向脚本
    /// </summary>
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        _facingRight = !_facingRight;
        // Multiply the player's x local scale by -1.
        // 通过scale来设置左右的动画
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// 跳跃脚本
    /// </summary>
    /// <param name="jump"></param>
    private void Jump(bool jump)
    {
        // 按键触发了跳跃
        if (jump)
        {
            // 当前剩余跳跃次数
            if (_jumpCount > 0)
            {
                // 跳跃受力
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce);
                // 减少次数
                _jumpCount--;
                // 清除降落状态
                if (_isLanded) _isLanded = false;
            }
        }
    }

    /// <summary>
    /// 地面检测
    /// </summary>
    private void CheckLanded()
    {
        // 初始状态
        bool wasGrounded = _isGrounded;
        // 
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _ground);
        // 落地之后
        if (_isGrounded && !wasGrounded)
        {
            _isLanded = true;
            // 受伤后着地则结束受伤
            if (_isHurt) { _isHurt = false; }
            // 重置跳跃次数
            _jumpCount = _jumpDefaultCount;
        }
        //
        //Debug.Log(Mathf.Abs(_rigidbody2D.velocity.x));
        //if (_isHurt && Mathf.Abs(_rigidbody2D.velocity.x) < 0.1f) { _isHurt = false; }
        //
        //GameObject[] targetObjects = System.Array.ConvertAll(Physics.OverlapCircleAll(_groundCheck.position, _groundCheckRadius, _ground, collider => collider.gameObject);
    }

    // 碰撞检测
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "enemy")
        {
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            // 下落时消灭敌人
            if (_animator.GetBool("isFall"))
            {
                enemy.Jumpon();
            }
            // 受伤后移动
            else
            {
                if (transform.position.x < other.gameObject.transform.position.x)
                {
                    _isHurt = true;
                    _rigidbody2D.velocity = new Vector2(-1 * _hurtForce, _hurtForce);
                }
                else if (transform.position.x > other.gameObject.transform.position.x)
                {
                    _isHurt = true;
                    _rigidbody2D.velocity = new Vector2(_hurtForce, _hurtForce);
                }
                //
                if (_isHurt)
                {
                    GameController.Instance.UpdateHealth(-1);
                    GameController.Instance.RefreshGameInfos();
                    //
                    BlinkPlayer(blinks, blinkTime);
                }
            }
        }
    }
    void BlinkPlayer(int numBlinks, float seconds)
    {
        StartCoroutine(DoBlinks(numBlinks, seconds));
    }
    IEnumerator DoBlinks(int numBlinks, float seconds)
    {
        for (int i = 0; i < numBlinks * 2; i++)
        {
            _render.enabled = !_render.enabled;
            yield return new WaitForSeconds(seconds);
        }
        _render.enabled = true;
    }

    void SwitchAnimation()
    {
        // 奔跑
        _animator.SetFloat("running", Mathf.Abs(_running));
        // 下蹲
        _animator.SetBool("isCrouch", _isCrouch);
        // 跳跃|降落
        _animator.SetBool("isJump", !_isGrounded);
        _animator.SetBool("isFall", false);
        if (!_isGrounded && _rigidbody2D.velocity.y < 0)
        {
            _animator.SetBool("isJump", false);
            _animator.SetBool("isFall", true);
        }
        // 重返地面
        if (_isLanded)
        {
            _animator.SetTrigger("triLanded");
        }
        // 受伤
        _animator.SetBool("isHurt", _isHurt);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        // 地面检测
        Gizmos.color = Color.green;
        if (_isGrounded) { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        // 头顶检测
        Gizmos.color = Color.red;
        if (_canStand) { Gizmos.color = Color.green; }
        Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
    }
}
