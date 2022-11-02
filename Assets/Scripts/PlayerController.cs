using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D; // ����
    private Animator _animator; // ����������
    private Transform _transform; // Transform���
    private Renderer _render; // ���ͼƬ��ʾ

    [HideInInspector] public AudioSource _jumpAudio; // ��Ƶ������
    [HideInInspector] public List<AudioClip> list; // ���������Ҫ���ŵ���Ƶ�б�

    [HideInInspector] public float _healthMax = 5; // ���Ѫ��
    private float health; // ��ǰѪ��
    // �����߽�
    private float deadLiney = -7.0f;

    private bool canInput = true; // �ܷ�����ָ�����

    [Header("MOVE")] // [Move] Config
    public float _moveSpeed = 6f; // �ƶ��ٶ�
    [HideInInspector][Range(0, 0.05f)] public float _moveSmoothing = 0.05f; // �ƶ�˳����
    [SerializeField] private float moveState = 0; // �ƶ�״̬
    private bool facingRight = true;  // �Ƿ�������

    [Header("JUMP")] // [Jump] Config
    [HideInInspector] public int _jumpMax = 2; // �����Ծ����
    [HideInInspector] public float _jumpSpeed = 8f; // ��Ծ�ٶ�
    [HideInInspector] public bool _canAirControl = true; // �ڿ����Ƿ����ת��
    private bool jumpPressed = false; // �Ƿ�����Ծ��
    private float lastJumpPressed;
    private bool isJump = false; // �Ƿ�����أ��ܷ���Ծ
    private int jumpCount = 0; // ��ǰ��Ծ����
    private bool isGrounded = false; // �Ƿ�����أ��ܷ���Ծ
    private bool isLanded; // �Ƿ���
    private float _fallingGravity = 2; // ������ٶȱ���
    // Ground Config
    [HideInInspector] public LayerMask _ground; // �����ж�Ϊ�����ͼ��
    [HideInInspector] public Transform _groundCheck; // �ŵ��жϽӴ��ļ���
    const float _groundCheckRadius = .2f; // �ŵ��жϰ뾶

    [Header("CROUCH")] // [Crouch] Config
    [HideInInspector] public Collider2D _ceilingCollider; // ͷ����ײ�壬�¶�ʱʧЧ
    [HideInInspector] public Transform _ceilingCheck; // ͷ���жϽӴ��ļ���
    [HideInInspector] public float _ceilingCheckRadius = .2f; // ͷ���жϰ뾶
    [HideInInspector] public float _crouchSpeedRate = 0.36f; // �¶׵��ƶ��ٶ�
    private bool crouchPressed = false; // �Ƿ����¶׼�
    private bool isCrouch; // Whether or not the player is crouched
    private bool canStand; // �ж��Ƿ��վ��

    // [Attack] Config
    [HideInInspector] public float _attckDamgeLight; // �ṥ���˺�
    [HideInInspector] public float _attckDamgeHeavy; // �ع����˺�
    [HideInInspector] public float _attackSpeedLight; // �ṥ�������ٶ�
    [HideInInspector] public float _attackSpeedHeavy; // �ع��������ٶ�
    private bool isAttack; // �Ƿ��ڹ���״̬
    private int attackMode; // ����ģʽ
    private int attackComboCount;// ��ǰ����������

    // [Hurt][Defend] Config
    [HideInInspector] public float _hurtForce = 2f; // ����ʱ�������ٶ�
    [HideInInspector] public float _defendDefaultTime = 3f; // Ĭ���޵�ʱ��
    private bool isHurt = false; // �Ƿ�������״̬
    private bool isDefend = false; // �Ƿ����޵�״̬
    //
    [HideInInspector] public float _blinkTime; // ��˸���
    private int blinkCount = 0; // ��˸����

    // [Sliding] Config
    [HideInInspector] public float _slidSpeed = 6f; // �����ٶ�
    private float canSliding; // �ܷ�����

    // PressKey
    public FrameInput inputInfos { get; private set; }
    private float x, y;
    private float xRaw, yRaw;
    private Vector3 velocity = Vector3.zero;

    // ��ʼ��
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
        // [Move]��ȡ����
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");
        // [Crouch]����C���¶ף��ɿ��ָ�
        if (Input.GetButtonDown("Crouch")) crouchPressed = true;
        else if (Input.GetButtonUp("Crouch")) crouchPressed = false;
        // [Jump]�����ո����Ծ
        if (Input.GetButtonDown("Jump")) jumpPressed = true;
        //
        if (Input.GetKeyDown(KeyCode.LeftShift)) dashPressed = true;
        // [Log]
        Debug.Log("x:" + xRaw + "; y:" + yRaw + "; C:" + crouchPressed + "; Space:" + jumpPressed + ";");
        // ��ɫ���䣬������Ϸ
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
        // [Move]�ƶ�
        Movement();
        // [Jump]��Ծ
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

    /// [Move][Crouch]�ƶ��ű�
    private void Movement()
    {
        // ���ɲ���״̬������״̬ʱ�������ƶ�
        if (!canInput || isHurt) return;
        // �û�����״̬
        moveState = xRaw;
        // �ж��Ƿ��Ѿ�����վ���ˣ�������״̬
        canStand = !Physics2D.OverlapCircle(_ceilingCheck.position, _ceilingCheckRadius, _ground);
        isCrouch = crouchPressed || !canStand;
        // �¶�
        if (isCrouch)
        {
            // ����Ϊ�¶�״̬���ٶ��ƶ�
            moveState *= _crouchSpeedRate;
            // �¶�ʱ��ͷ������ײ��ʧЧ
            if (_ceilingCollider != null) _ceilingCollider.enabled = false;
        }
        else
        {
            // �ָ�ͷ����ײ��
            if (_ceilingCollider != null) _ceilingCollider.enabled = true;
        }
        // �����ƶ�������������˳����Ӧ�õ���ɫ����
        Vector3 targetVelocity = new Vector2(moveState * _moveSpeed, _rigidbody2D.velocity.y);
        _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, _moveSmoothing);
        // ��ɫ�ڵ����� �� �������� �ſ�ת��
        if (isGrounded || _canAirControl)
        {
            if ((moveState > 0 && !facingRight) || (moveState < 0 && facingRight)) Flip();
        }
    }

    // [Jump]��Ծ
    private void Jump()
    {
        // ���ɲ���״̬������״̬ʱ��������Ծ
        if (!canInput) return;
        // ������������Ծ
        if (jumpPressed)
        {
            // ������Ծ״̬
            jumpPressed = false;
            // ½����Ծ
            if (isGrounded)
            {
                isJump = true;
                jumpCount = 0;
                // ��Ծ����
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpSpeed);
                jumpCount++;
            }
            // �����Ծ
            else if (jumpCount > 0 && jumpCount < _jumpMax)
            {
                //
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpSpeed);
                jumpCount++;
                // �������״̬
                if (isLanded) isLanded = false;
            }
        }
        //
        if (!isGrounded && _rigidbody2D.velocity.y < 0)
        {
            //isFalling = true;
        }
        // �������
        if (_rigidbody2D.velocity.y < 0) _rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (_fallingGravity - 1) * Time.fixedDeltaTime;
    }
    // ���ذ�
    private void CheckGround()
    {
        // �����ص�ǰ��״̬�仯
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _ground);
        // ���֮��
        if (isGrounded && !wasGrounded)
        {
            isLanded = true;
            // ���˺��ŵ����������
            if (isHurt) { isHurt = false; }
        }
    }

    // [Flip]��ת��ɫ
    private void Flip()
    {
        facingRight = !facingRight;
        // ͨ��scale���������ҵĶ���
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Attack() // ����
    {
        if (!isAttack && !isHurt)
        {
            isAttack = true;
            canInput = false;
            //
            attackMode = 1;
            _rigidbody2D.velocity = Vector2.zero;
            // ��������
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
    public float _dashSpeed; // ����ٶ�
    public float _dashTimeMax; // ��̳���ʱ��
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
            else // �������
            {
                isDashing = false;
                dashObject.SetActive(false);
            }
        }
    }

    // [AfterImage] Config
    private Transform tf_SpawnPosition; // ��Ӱ����λ��
    private Color cr_AfterImageColor = Color.black; // ��ɫ
    private float maxAfterTime = 1f; // ������Ӱ����ʱ��
    private float maxIntervalSpawnTime = 0.2f; // ������Ӱ���ʱ��
    private float maxLiveTime = 0.5f; // ��Ӱ����ʱ��
    private bool startSpawn = false; // �Ƿ�ʼ����
    float duringAfterTime = 0;
    float duringIntervalSpawnTime = 0;
    List<AfterImage> afterImages; // ������в����˵Ĳ�Ӱ�����������ɾ����
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

    // ��ײ���
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "enemy")
        {
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            // ����ʱ�������
            if (_animator.GetBool("isFall"))
            {
                enemy.Jumpon();
                // ������˺�����
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpSpeed / 2);
            }
            // ���˺��ƶ�
            else
            {
                // ���˵�����
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
                    // ����Ч��
                    GameController.Instance.UpdateHealth(-1);
                    GameController.Instance.RefreshGameInfos();
                    //
                    BlinkPlayer(blinkCount, _blinkTime);
                }
            }
        }
    }

    // �л�����
    void SwitchAnimation()
    {
        // ����
        _animator.SetFloat("running", Mathf.Abs(moveState));
        // �¶�
        _animator.SetBool("isCrouch", isCrouch);
        // ��Ծ
        _animator.SetBool("isJump", !isGrounded);
        _animator.SetBool("isFall", false);
        // ����
        if (!isGrounded && _rigidbody2D.velocity.y < 0)
        {
            _animator.SetBool("isJump", false);
            _animator.SetBool("isFall", true);
        }
        // ��ػָ�״̬
        if (isLanded)
        {
            _animator.SetTrigger("triLanded");
        }
    }

    //
    private void OnDrawGizmos()
    {
        // ���������ʾ
        Gizmos.color = Color.green;
        if (isGrounded) { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        // ͷ��������ʾ
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

    // ��������ʱ����͸����
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