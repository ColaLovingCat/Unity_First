using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private Renderer _render;
    public AudioSource _jumpAudio;

    // Move Config
    [HideInInspector][SerializeField] private float running; // 记录移动的状态
    //
    [HideInInspector][SerializeField] private float _movementSpeed = 6f; // 移动速度
    [HideInInspector][Range(0, 0.3f)][SerializeField] private float _movementSmoothing = 0.05f; // How much to smooth out the movement

    // Jump Info
    private bool jumpPressed = false; // 是否按下跳跃键
    [HideInInspector][SerializeField] private int jumpCount; // 可跳跃次数记录
    private bool isLanded;
    //
    [HideInInspector][Range(0, 8f)][SerializeField] private float _jumpForce = 8f; // 跳跃力度
    [HideInInspector][SerializeField] private int _jumpAllowCount = 2; // 最大跳跃次数
    [HideInInspector][SerializeField] private bool _canAirControl = false; // 在空中是否可以转向
    [HideInInspector][SerializeField] private float fallGravityMultiplier = 2; // 下落加速度倍数
    //
    [SerializeField] private bool isGrounded = false;
    //
    [HideInInspector] public LayerMask _ground; // 用于判断为地面的图层
    [HideInInspector][SerializeField] public Transform _groundCheck; // 脚底判断接触的检查点
    const float _groundCheckRadius = .2f; // 脚底判断半径

    // Crouch Info
    private bool isCrouch, _canStand; // Whether or not the player is crouched
    //
    [HideInInspector][Range(0, 1)][SerializeField] private float _crouchSpeed = 0.36f; // 下蹲的移动速度
    // Ceiling Info
    [HideInInspector][SerializeField] public Collider2D _colliderCeilingDisable; // 头部碰撞体，下蹲时失效
    [HideInInspector][SerializeField] public Transform _ceilingCheck; // 头顶判断接触的检查点
    const float _ceilingCheckRadius = .2f; // 头顶判断半径

    private Vector3 velocity = Vector3.zero;
    private bool _facingRight = true;  // For determining which way the player is currently facing.

    private bool isHurt; // 角色受伤状态

    public float blinkTime;
    public int blinks;
    [HideInInspector][SerializeField] private float _hurtForce = 2f;

    // 按键监测
    private float x, y;
    private float xRaw, yRaw;
    // 角色状态
    private bool crouchPressed = false;

    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _render = GetComponent<Renderer>();
    }

    void Start()
    {
        // [Jump]初始化跳跃次数
        jumpCount = _jumpAllowCount;
    }

    void Update()
    {
        // 获取按键
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");
        // 长按C键下蹲，松开恢复
        if (Input.GetButtonDown("Crouch"))
            crouchPressed = true;
        else if (Input.GetButtonUp("Crouch"))
            crouchPressed = false;
        // [Jump]单击空格键跳跃
        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        // 重置落地状态
        isLanded = false;
        // 地面检测
        CheckLanded();
        //
        Move(xRaw, crouchPressed);
        // [Jump]
        Jump();
        // [Jump]下落加速
        if (_rigidbody2D.velocity.y < 0) _rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
    }

    /// <summary>
    /// 暴露在外的移动方法
    /// </summary>
    /// <param name="move">左右移动方向</param>
    /// <param name="crouch">是否下蹲</param>
    /// <param name="jump">是否跳跃</param>
    public void Move(float move, bool crouch)
    {
        // 没有受伤时才能操作
        if (!isHurt)
        {
            Movement(move, crouch);
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
        isCrouch = crouch;
        // 角色在地面上
        if (isGrounded)
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
            running = move;
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * _movementSpeed, _rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, _movementSmoothing);
        }
        // 角色在地面上 或 空中允许 才可转向
        if (isGrounded || _canAirControl)
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
    private void Jump()
    {
        // 按键触发了跳跃
        if (jumpPressed)
        {
            jumpPressed = false;
            // 当前剩余跳跃次数
            if (jumpCount < 1) return;
            else
            {
                // 跳跃受力
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce);
                // 减少次数
                jumpCount--;
                // 清除降落状态
                if (isLanded) isLanded = false;
            }
        }
    }
    private void CheckLanded()
    {
        // 初始状态
        bool wasGrounded = isGrounded;
        // 
        isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _ground);
        // 落地之后
        if (isGrounded && !wasGrounded)
        {
            isLanded = true;
            // 受伤后着地则结束受伤
            if (isHurt) { isHurt = false; }
            // 重置跳跃次数
            jumpCount = _jumpAllowCount;
        }
        //
        //Debug.Log(Mathf.Abs(_rigidbody2D.velocity.x));
        //if (isHurt && Mathf.Abs(_rigidbody2D.velocity.x) < 0.1f) { isHurt = false; }
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
                // 消灭敌人后跳起
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce / 2);
            }
            // 受伤后移动
            else
            {
                // 受伤的受力
                if (transform.position.x < other.gameObject.transform.position.x)
                {
                    isHurt = true;
                    _rigidbody2D.velocity = new Vector2(-1 * _hurtForce, _hurtForce);
                }
                else if (transform.position.x > other.gameObject.transform.position.x)
                {
                    isHurt = true;
                    _rigidbody2D.velocity = new Vector2(_hurtForce, _hurtForce);
                }
                //
                if (isHurt)
                {
                    // 受伤效果
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
        _animator.SetFloat("running", Mathf.Abs(running));
        // 下蹲
        _animator.SetBool("isCrouch", isCrouch);
        // 跳跃|降落
        _animator.SetBool("isJump", !isGrounded);
        _animator.SetBool("isFall", false);
        if (!isGrounded && _rigidbody2D.velocity.y < 0)
        {
            _animator.SetBool("isJump", false);
            _animator.SetBool("isFall", true);
        }
        // 重返地面
        if (isLanded)
        {
            _animator.SetTrigger("triLanded");
        }
        //
        if (isHurt && Mathf.Abs(_rigidbody2D.velocity.x) < 0.1f) { isHurt = false; }
        // 受伤
        _animator.SetBool("isHurt", isHurt);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        // 地面检测
        Gizmos.color = Color.green;
        if (isGrounded) { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        // 头顶检测
        Gizmos.color = Color.red;
        if (_canStand) { Gizmos.color = Color.green; }
        Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
    }
}
