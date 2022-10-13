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
    [HideInInspector][SerializeField] private bool _canAirControl = false; // �ڿ����Ƿ����ת��
    [HideInInspector][SerializeField] private int _jumpDefaultCount = 2; // Whether or not a player can steer while jumping;
    [HideInInspector][SerializeField] private int _jumpCount; //
    private bool _isLanded; // Whether or not a player can steer while jumping;
    // Ground Info
    [SerializeField] private bool _isGrounded = false;
    [HideInInspector][SerializeField] private LayerMask _ground; // �����жϵ����ͼ��
    [HideInInspector][SerializeField] private Transform _groundCheck; // �����жϵ�
    const float _groundCheckRadius = .2f; // �����жϰ뾶

    // Crouch Info
    private bool _isCrouch, _canStand; // Whether or not the player is crouched
    [HideInInspector][Range(0, 1)][SerializeField] private float _crouchSpeed = 0.36f; // �¶׵��ƶ��ٶ�
    // Ceiling Info
    [HideInInspector][SerializeField] private Collider2D _colliderCeilingDisable; // ͷ����ײ�壬�¶�ʱʧЧ
    [HideInInspector][SerializeField] public Transform _ceilingCheck; // ͷ���жϵ�
    const float _ceilingCheckRadius = .2f; // ͷ���жϰ뾶

    private Vector3 velocity = Vector3.zero;
    private bool _facingRight = true;  // For determining which way the player is currently facing.

    private bool _isHurt; // ��ɫ����״̬
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
        // �������״̬
        _isLanded = false;
        // ������
        CheckLanded();
    }

    /// <summary>
    /// ��¶������ƶ�����
    /// </summary>
    /// <param name="move">�����ƶ�����</param>
    /// <param name="crouch">�Ƿ��¶�</param>
    /// <param name="jump">�Ƿ���Ծ</param>
    public void Move(float move, bool crouch, bool jump)
    {
        // û������ʱ���ܲ���
        if (!_isHurt)
        {
            Movement(move, crouch);
            Jump(jump);
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
        _isCrouch = crouch;
        // ��ɫ�ڵ�����
        if (_isGrounded)
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
            _running = move;
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * _movementSpeed, _rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, _movementSmoothing);
        }
        // ��ɫ�ڵ����� �� �������� �ſ�ת��
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
    /// <param name="jump"></param>
    private void Jump(bool jump)
    {
        // ������������Ծ
        if (jump)
        {
            // ��ǰʣ����Ծ����
            if (_jumpCount > 0)
            {
                // ��Ծ����
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce);
                // ���ٴ���
                _jumpCount--;
                // �������״̬
                if (_isLanded) _isLanded = false;
            }
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    private void CheckLanded()
    {
        // ��ʼ״̬
        bool wasGrounded = _isGrounded;
        // 
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _ground);
        // ���֮��
        if (_isGrounded && !wasGrounded)
        {
            _isLanded = true;
            // ���˺��ŵ����������
            if (_isHurt) { _isHurt = false; }
            // ������Ծ����
            _jumpCount = _jumpDefaultCount;
        }
        //
        //Debug.Log(Mathf.Abs(_rigidbody2D.velocity.x));
        //if (_isHurt && Mathf.Abs(_rigidbody2D.velocity.x) < 0.1f) { _isHurt = false; }
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
            }
            // ���˺��ƶ�
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
        // ����
        _animator.SetFloat("running", Mathf.Abs(_running));
        // �¶�
        _animator.SetBool("isCrouch", _isCrouch);
        // ��Ծ|����
        _animator.SetBool("isJump", !_isGrounded);
        _animator.SetBool("isFall", false);
        if (!_isGrounded && _rigidbody2D.velocity.y < 0)
        {
            _animator.SetBool("isJump", false);
            _animator.SetBool("isFall", true);
        }
        // �ط�����
        if (_isLanded)
        {
            _animator.SetTrigger("triLanded");
        }
        // ����
        _animator.SetBool("isHurt", _isHurt);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        // ������
        Gizmos.color = Color.green;
        if (_isGrounded) { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        // ͷ�����
        Gizmos.color = Color.red;
        if (_canStand) { Gizmos.color = Color.green; }
        Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
    }
}
