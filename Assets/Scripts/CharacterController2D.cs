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
    [HideInInspector][SerializeField] private float running; // ��¼�ƶ���״̬
    //
    [HideInInspector][SerializeField] private float _movementSpeed = 6f; // �ƶ��ٶ�
    [HideInInspector][Range(0, 0.3f)][SerializeField] private float _movementSmoothing = 0.05f; // How much to smooth out the movement

    // Jump Info
    private bool jumpPressed = false; // �Ƿ�����Ծ��
    [HideInInspector][SerializeField] private int jumpCount; // ����Ծ������¼
    private bool isLanded;
    //
    [HideInInspector][Range(0, 8f)][SerializeField] private float _jumpForce = 8f; // ��Ծ����
    [HideInInspector][SerializeField] private int _jumpAllowCount = 2; // �����Ծ����
    [HideInInspector][SerializeField] private bool _canAirControl = false; // �ڿ����Ƿ����ת��
    [HideInInspector][SerializeField] private float fallGravityMultiplier = 2; // ������ٶȱ���
    //
    [SerializeField] private bool isGrounded = false;
    //
    [HideInInspector] public LayerMask _ground; // �����ж�Ϊ�����ͼ��
    [HideInInspector][SerializeField] public Transform _groundCheck; // �ŵ��жϽӴ��ļ���
    const float _groundCheckRadius = .2f; // �ŵ��жϰ뾶

    // Crouch Info
    private bool isCrouch, _canStand; // Whether or not the player is crouched
    //
    [HideInInspector][Range(0, 1)][SerializeField] private float _crouchSpeed = 0.36f; // �¶׵��ƶ��ٶ�
    // Ceiling Info
    [HideInInspector][SerializeField] public Collider2D _colliderCeilingDisable; // ͷ����ײ�壬�¶�ʱʧЧ
    [HideInInspector][SerializeField] public Transform _ceilingCheck; // ͷ���жϽӴ��ļ���
    const float _ceilingCheckRadius = .2f; // ͷ���жϰ뾶

    private Vector3 velocity = Vector3.zero;
    private bool _facingRight = true;  // For determining which way the player is currently facing.

    private bool isHurt; // ��ɫ����״̬

    public float blinkTime;
    public int blinks;
    [HideInInspector][SerializeField] private float _hurtForce = 2f;

    // �������
    private float x, y;
    private float xRaw, yRaw;
    // ��ɫ״̬
    private bool crouchPressed = false;

    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _render = GetComponent<Renderer>();
    }

    void Start()
    {
        // [Jump]��ʼ����Ծ����
        jumpCount = _jumpAllowCount;
    }

    void Update()
    {
        // ��ȡ����
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");
        // ����C���¶ף��ɿ��ָ�
        if (Input.GetButtonDown("Crouch"))
            crouchPressed = true;
        else if (Input.GetButtonUp("Crouch"))
            crouchPressed = false;
        // [Jump]�����ո����Ծ
        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        // �������״̬
        isLanded = false;
        // ������
        CheckLanded();
        //
        Move(xRaw, crouchPressed);
        // [Jump]
        Jump();
        // [Jump]�������
        if (_rigidbody2D.velocity.y < 0) _rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
    }

    /// <summary>
    /// ��¶������ƶ�����
    /// </summary>
    /// <param name="move">�����ƶ�����</param>
    /// <param name="crouch">�Ƿ��¶�</param>
    /// <param name="jump">�Ƿ���Ծ</param>
    public void Move(float move, bool crouch)
    {
        // û������ʱ���ܲ���
        if (!isHurt)
        {
            Movement(move, crouch);
        }
        //
        SwitchAnimation();
    }

    /// <summary>
    /// �ƶ��ű�
    /// </summary>
    /// <param name="move"></param>
    /// <param name="crouch"></param>
    private void Movement(float move, bool crouch)
    {
        // ����վ��״̬
        _canStand = true;
        // ͷ�����
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
        // ��ɫ�ڵ�����
        if (isGrounded)
        {
            // If crouching
            if (crouch)
            {
                // ���¶׵��ٶ��ƶ�
                move *= _crouchSpeed;
                // ���¶�ʱ��ͷ������ײ��ʧЧ
                if (_colliderCeilingDisable != null) _colliderCeilingDisable.enabled = false;
            }
            else
            {
                // �ָ�ͷ����ײ��
                if (_colliderCeilingDisable != null) _colliderCeilingDisable.enabled = true;
            }
            //
            running = move;
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * _movementSpeed, _rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, _movementSmoothing);
        }
        // ��ɫ�ڵ����� �� �������� �ſ�ת��
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
    /// ת��ű�
    /// </summary>
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        _facingRight = !_facingRight;
        // Multiply the player's x local scale by -1.
        // ͨ��scale���������ҵĶ���
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// ��Ծ�ű�
    /// </summary>
    private void Jump()
    {
        // ������������Ծ
        if (jumpPressed)
        {
            jumpPressed = false;
            // ��ǰʣ����Ծ����
            if (jumpCount < 1) return;
            else
            {
                // ��Ծ����
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce);
                // ���ٴ���
                jumpCount--;
                // �������״̬
                if (isLanded) isLanded = false;
            }
        }
    }
    private void CheckLanded()
    {
        // ��ʼ״̬
        bool wasGrounded = isGrounded;
        // 
        isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _ground);
        // ���֮��
        if (isGrounded && !wasGrounded)
        {
            isLanded = true;
            // ���˺��ŵ����������
            if (isHurt) { isHurt = false; }
            // ������Ծ����
            jumpCount = _jumpAllowCount;
        }
        //
        //Debug.Log(Mathf.Abs(_rigidbody2D.velocity.x));
        //if (isHurt && Mathf.Abs(_rigidbody2D.velocity.x) < 0.1f) { isHurt = false; }
        //
        //GameObject[] targetObjects = System.Array.ConvertAll(Physics.OverlapCircleAll(_groundCheck.position, _groundCheckRadius, _ground, collider => collider.gameObject);
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
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce / 2);
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
        // ����
        _animator.SetFloat("running", Mathf.Abs(running));
        // �¶�
        _animator.SetBool("isCrouch", isCrouch);
        // ��Ծ|����
        _animator.SetBool("isJump", !isGrounded);
        _animator.SetBool("isFall", false);
        if (!isGrounded && _rigidbody2D.velocity.y < 0)
        {
            _animator.SetBool("isJump", false);
            _animator.SetBool("isFall", true);
        }
        // �ط�����
        if (isLanded)
        {
            _animator.SetTrigger("triLanded");
        }
        //
        if (isHurt && Mathf.Abs(_rigidbody2D.velocity.x) < 0.1f) { isHurt = false; }
        // ����
        _animator.SetBool("isHurt", isHurt);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        // ������
        Gizmos.color = Color.green;
        if (isGrounded) { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        // ͷ�����
        Gizmos.color = Color.red;
        if (_canStand) { Gizmos.color = Color.green; }
        Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
    }
}
