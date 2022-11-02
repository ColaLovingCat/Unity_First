using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D; // 刚体
    private Animator _animator; // 动画控制器
    private Transform _transform; // Transform组件
    private Renderer _render; // 玩家图片显示

    [HideInInspector] public AudioSource _jumpAudio; // 音频播放器
    [HideInInspector] public List<AudioClip> list; // 玩家身上需要播放的音频列表

    [HideInInspector] public float _healthMax = 5; // 最大血量
    private float health; // 当前血量
    // 死亡边界
    private float deadLiney = -7.0f;

    private bool canInput = true; // 能否输入指令操作

    [Header("MOVE")] // [Move] Config
    public float _moveSpeed = 6f; // 移动速度
    [HideInInspector][Range(0, 0.05f)] public float _moveSmoothing = 0.05f; // 移动顺滑度
    [SerializeField] private float moveState = 0; // 移动状态
    private bool facingRight = true;  // 是否面向右

    [Header("JUMP")] // [Jump] Config
    [HideInInspector] public int _jumpMax = 2; // 最大跳跃次数
    [HideInInspector] public float _jumpSpeed = 8f; // 跳跃速度
    [HideInInspector] public bool _canAirControl = true; // 在空中是否可以转向
    private bool jumpPressed = false; // 是否按下跳跃键
    private float lastJumpPressed;
    private bool isJump = false; // 是否已落地，能否跳跃
    private int jumpCount = 0; // 当前跳跃次数
    private bool isGrounded = false; // 是否已落地，能否跳跃
    private bool isLanded; // 是否降落
    private float _fallingGravity = 2; // 下落加速度倍数
    // Ground Config
    [HideInInspector] public LayerMask _ground; // 用于判断为地面的图层
    [HideInInspector] public Transform _groundCheck; // 脚底判断接触的检查点
    const float _groundCheckRadius = .2f; // 脚底判断半径

    [Header("CROUCH")] // [Crouch] Config
    [HideInInspector] public Collider2D _ceilingCollider; // 头部碰撞体，下蹲时失效
    [HideInInspector] public Transform _ceilingCheck; // 头顶判断接触的检查点
    [HideInInspector] public float _ceilingCheckRadius = .2f; // 头顶判断半径
    [HideInInspector] public float _crouchSpeedRate = 0.36f; // 下蹲的移动速度
    private bool crouchPressed = false; // 是否按下下蹲键
    private bool isCrouch; // Whether or not the player is crouched
    private bool canStand; // 判断是否可站立

    // [Attack] Config
    [HideInInspector] public float _attckDamgeLight; // 轻攻击伤害
    [HideInInspector] public float _attckDamgeHeavy; // 重攻击伤害
    [HideInInspector] public float _attackSpeedLight; // 轻攻击补偿速度
    [HideInInspector] public float _attackSpeedHeavy; // 重攻击补偿速度
    private bool isAttack; // 是否处于攻击状态
    private int attackMode; // 攻击模式
    private int attackComboCount;// 当前攻击连击数

    // [Hurt][Defend] Config
    [HideInInspector] public float _hurtForce = 2f; // 受伤时反弹的速度
    [HideInInspector] public float _defendDefaultTime = 3f; // 默认无敌时间
    private bool isHurt = false; // 是否处于受伤状态
    private bool isDefend = false; // 是否处于无敌状态
    //
    [HideInInspector] public float _blinkTime; // 闪烁间隔
    private int blinkCount = 0; // 闪烁计数

    // [Sliding] Config
    [HideInInspector] public float _slidSpeed = 6f; // 闪避速度
    private float canSliding; // 能否闪避

    // PressKey
    public FrameInput inputInfos { get; private set; }
    private float x, y;
    private float xRaw, yRaw;
    private Vector3 velocity = Vector3.zero;

    // 初始化
    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _render = GetComponent<Renderer>();
    }

    void Start()
    {
        afterImages = new List<AfterImage>();
    }

    void Update()
    {
        GatherInput();
        // [Move]获取按键
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");
        // [Crouch]长按C键下蹲，松开恢复
        if (Input.GetButtonDown("Crouch")) crouchPressed = true;
        else if (Input.GetButtonUp("Crouch")) crouchPressed = false;
        // [Jump]单击空格键跳跃
        if (Input.GetButtonDown("Jump")) jumpPressed = true;
        //
        if (Input.GetKeyDown(KeyCode.LeftShift)) dashPressed = true;
        // [Log]
        Debug.Log("x:" + xRaw + "; y:" + yRaw + "; C:" + crouchPressed + "; Space:" + jumpPressed + ";");
        // 角色掉落，重置游戏
        if (transform.position.y < deadLiney)
        {
            GetComponent<AudioSource>().enabled = false;
            GameController.Instance.Restart();
        }
    }

    void FixedUpdate()
    {
        isLanded = false;
        CheckGround();
        // [Move]移动
        Movement();
        // [Jump]跳跃
        Jump();
        //
        Dash();
        checkAfterImage();
        //
        SwitchAnimation();
    }

    private void GatherInput()
    {
        inputInfos = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump"),
            JumpUp = Input.GetButtonUp("Jump"),
            xRaw = Input.GetAxisRaw("Horizontal"),
        };
        if (inputInfos.JumpDown)
        {
            lastJumpPressed = Time.time;
        }
    }

    /// [Move][Crouch]移动脚本
    private void Movement()
    {
        // 不可操作状态或受伤状态时，不可移动
        if (!canInput || isHurt) return;
        // 用户输入状态
        moveState = xRaw;
        // 判断是否已经可以站立了，并更新状态
        canStand = !Physics2D.OverlapCircle(_ceilingCheck.position, _ceilingCheckRadius, _ground);
        isCrouch = crouchPressed || !canStand;
        // 下蹲
        if (isCrouch)
        {
            // 更新为下蹲状态的速度移动
            moveState *= _crouchSpeedRate;
            // 下蹲时将头部的碰撞器失效
            if (_ceilingCollider != null) _ceilingCollider.enabled = false;
        }
        else
        {
            // 恢复头部碰撞器
            if (_ceilingCollider != null) _ceilingCollider.enabled = true;
        }
        // 左右移动向量，并将其顺滑的应用到角色身上
        Vector3 targetVelocity = new Vector2(moveState * _moveSpeed, _rigidbody2D.velocity.y);
        _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, _moveSmoothing);
        // 角色在地面上 或 空中允许 才可转向
        if (isGrounded || _canAirControl)
        {
            if ((moveState > 0 && !facingRight) || (moveState < 0 && facingRight)) Flip();
        }
    }

    // [Jump]跳跃
    private void Jump()
    {
        // 不可操作状态或受伤状态时，不可跳跃
        if (!canInput) return;
        // 按键触发了跳跃
        if (jumpPressed)
        {
            // 重置跳跃状态
            jumpPressed = false;
            // 陆地跳跃
            if (isGrounded)
            {
                isJump = true;
                jumpCount = 0;
                // 跳跃受力
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpSpeed);
                jumpCount++;
            }
            // 多次跳跃
            else if (jumpCount > 0 && jumpCount < _jumpMax)
            {
                //
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpSpeed);
                jumpCount++;
                // 清除降落状态
                if (isLanded) isLanded = false;
            }
        }
        //
        if (!isGrounded && _rigidbody2D.velocity.y < 0)
        {
            //isFalling = true;
        }
        // 下落加速
        if (_rigidbody2D.velocity.y < 0) _rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (_fallingGravity - 1) * Time.fixedDeltaTime;
    }
    // 检测地板
    private void CheckGround()
    {
        // 检测落地的前后状态变化
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _ground);
        // 落地之后
        if (isGrounded && !wasGrounded)
        {
            isLanded = true;
            // 受伤后着地则结束受伤
            if (isHurt) { isHurt = false; }
        }
    }

    // [Flip]反转角色
    private void Flip()
    {
        facingRight = !facingRight;
        // 通过scale来设置左右的动画
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Attack() // 攻击
    {
        if (!isAttack && !isHurt)
        {
            isAttack = true;
            canInput = false;
            //
            attackMode = 1;
            _rigidbody2D.velocity = Vector2.zero;
            // 连击计数
            attackComboCount++;
            if (attackComboCount > 3) attackComboCount = 1;
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

    // [Dash] Config
    public float _dashSpeed; // 冲刺速度
    public float _dashTimeMax; // 冲刺持续时间
    private bool dashPressed = false;
    private float dashStartTimer;
    private bool isDashing;
    public GameObject dashObject;
    //
    public void Dash()
    {
        if (!isDashing)
        {
            if (dashPressed)
            {
                dashPressed = false;
                //
                dashObject.SetActive(true);
                isDashing = true;
                dashStartTimer = _dashTimeMax;
            }
        }
        else
        {
            dashStartTimer -= Time.deltaTime;
            if (dashStartTimer > 0)
            {
                _rigidbody2D.velocity = _transform.right * _dashSpeed;
            }
            else // 结束冲刺
            {
                isDashing = false;
                dashObject.SetActive(false);
            }
        }
    }

    // [AfterImage] Config
    private Transform tf_SpawnPosition; // 残影产生位置
    private Color cr_AfterImageColor = Color.black; // 颜色
    private float maxAfterTime = 1f; // 产生残影持续时间
    private float maxIntervalSpawnTime = 0.2f; // 产生残影间隔时间
    private float maxLiveTime = 0.5f; // 残影生存时间
    private bool startSpawn = false; // 是否开始生成
    float duringAfterTime = 0;
    float duringIntervalSpawnTime = 0;
    List<AfterImage> afterImages; // 存放所有产生了的残影，方便管理与删除。
    // [AfterImage]
    public void checkAfterImage()
    {
        if (startSpawn)
        {
            if (duringAfterTime < maxAfterTime)
            {
                duringAfterTime += Time.deltaTime;
                if (duringIntervalSpawnTime < maxIntervalSpawnTime)
                {
                    duringIntervalSpawnTime += Time.deltaTime;
                }
                else
                {
                    duringIntervalSpawnTime = 0f;
                    spawnOne();
                }
            }
            else
            {
                startSpawn = false;
                duringAfterTime = 0f;
                duringIntervalSpawnTime = 0f;
            }
        }
        for (int i = 0; i < afterImages.Count; i++)
        {
            afterImages[i].update(Time.deltaTime);
        }
        while (afterImages.Count > 0 && afterImages[0].isDestroyed)
        {
            afterImages.RemoveAt(0);
        }
    }
    void spawnOne()
    {
        afterImages.Add(new AfterImage(gameObject, tf_SpawnPosition, cr_AfterImageColor, maxLiveTime));
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
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpSpeed / 2);
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
                    BlinkPlayer(blinkCount, _blinkTime);
                }
            }
        }
    }

    // 切换动画
    void SwitchAnimation()
    {
        // 奔跑
        _animator.SetFloat("running", Mathf.Abs(moveState));
        // 下蹲
        _animator.SetBool("isCrouch", isCrouch);
        // 跳跃
        _animator.SetBool("isJump", !isGrounded);
        _animator.SetBool("isFall", false);
        // 降落
        if (!isGrounded && _rigidbody2D.velocity.y < 0)
        {
            _animator.SetBool("isJump", false);
            _animator.SetBool("isFall", true);
        }
        // 落地恢复状态
        if (isLanded)
        {
            _animator.SetTrigger("triLanded");
        }
    }

    //
    private void OnDrawGizmos()
    {
        // 地面检测的显示
        Gizmos.color = Color.green;
        if (isGrounded) { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        // 头顶检测的显示
        Gizmos.color = Color.red;
        if (canStand) { Gizmos.color = Color.green; }
        Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
    }

}

class AfterImage
{
    GameObject gameObject;

    float liveTimeMax;
    float duringLiveTime = 0f;
    public bool isDestroyed = false;

    SpriteRenderer[] srList;

    public AfterImage(GameObject _gameObject, Transform _position, Color _color, float _liveTimeMax)
    {
        this.gameObject = GameObject.Instantiate(_gameObject, _position.transform.position, _position.transform.rotation);
        this.gameObject.transform.localScale = new Vector3(_position.localScale.x, _position.localScale.y, _position.localScale.z);

        this.liveTimeMax = _liveTimeMax;

        srList = this.gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in srList)
        {
            sr.color = _color;
            sr.sortingLayerName = "PerGround";
        }
    }

    // 更新生存时间与透明化
    public void update(float deltaTime)
    {
        if (duringLiveTime < liveTimeMax)
        {
            duringLiveTime += deltaTime;
            float alpha = 1f - duringLiveTime / liveTimeMax;
            foreach (SpriteRenderer sr in srList)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            }
        }
        else
        {
            GameObject.Destroy(gameObject);
            isDestroyed = true;
        }
    }
}

public struct FrameInput
{
    public float xRaw;
    public bool JumpDown;
    public bool JumpUp;
}