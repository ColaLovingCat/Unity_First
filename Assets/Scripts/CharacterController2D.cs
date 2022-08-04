using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    // 
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    public AudioSource _jumpAudio;

    // Move Info
    private float _running; // record moving status
    [SerializeField] private float _movementSpeed = 6f;
    [Range(0, 0.3f)] [SerializeField] private float _movementSmoothing = 0.05f; // How much to smooth out the movement

    // Jump Info
    [SerializeField] private float _jumpForce = 8f; // Amount of force added when the player jumps.
    [SerializeField] private bool _canAirControl = false; // Whether or not a player can steer while jumping;
    [Range(1, 2)] [SerializeField] private int _jumpDefaultCount = 2; // Whether or not a player can steer while jumping;
    private int _jumpCount; // Whether or not a player can steer while jumping;
    private bool _isLanded; // Whether or not a player can steer while jumping;
    // Ground Info
    private bool _isGrounded; // Whether or not the player is grounded
    [SerializeField] private LayerMask _ground; // A mask determining what is ground to the character
    [SerializeField] private Transform _groundCheck; // A position marking where to check if the player is grounded
    const float _groundCheckRadius = .2f; // Radius of the overlap circle to determine if grounded

    // Crouch Info
    private bool _isCrouch, _canStand; // Whether or not the player is crouched
    [Range(0, 1)] [SerializeField] private float _crouchSpeed = 0.36f; // Amount of maxSpeed applied to crouching movement. 1 = 100%
    // Ceiling Info
    [SerializeField] public Transform _ceilingCheck; // A position marking where to check for ceilings
    const float _ceilingCheckRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    [SerializeField] private Collider2D _colliderCeilingDisable; // A collider that will be disabled when crouching

    private Vector3 velocity = Vector3.zero;
    private bool _facingRight = true;  // For determining which way the player is currently facing.

    private bool _isHurt;
    [SerializeField] private float _hurtForce = 2f;

    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = false;
        _isLanded = false;
        // on ground
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheck.position, _groundCheckRadius, _ground);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                _isGrounded = true;
                // 着地变化
                if (!wasGrounded)
                {
                    _isLanded = true;
                    // 受伤后着地则结束受伤
                    if (_isHurt) { _isHurt = false; }
                }
            }
        }
    }

    public void Move(float move, bool crouch, bool jump)
    {
        if (!_isHurt)
        {
            Movement(move, crouch);
            Jump(jump);
        }
        //
        SwitchAnimation();
    }

    void Movement(float move, bool crouch)
    {
        _canStand = true;
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
        //only control the player if grounded or airControl is turned on
        if (_isGrounded || _canAirControl)
        {
            // If crouching
            if (crouch)
            {
                // Reduce the speed by the crouchSpeed multiplier
                move *= _crouchSpeed;
                // Disable one of the colliders when crouching
                if (_colliderCeilingDisable != null)
                    _colliderCeilingDisable.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (_colliderCeilingDisable != null)
                    _colliderCeilingDisable.enabled = true;
            }
            _running = move;
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * _movementSpeed, _rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, _movementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !_facingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && _facingRight)
            {
                // ... flip the player.
                Flip();
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

    private void Jump(bool jump)
    {
        // If the player should jump...
        if (_isLanded)
        {
            _jumpCount = _jumpDefaultCount;
        }
        if (jump && _isGrounded)
        {
            // Add a vertical force to the player
            _isGrounded = false;
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce);
            _jumpCount--;
        }
        // continue jump...
        //else if (jump && _jumpCount > 0)
        //{
        //    _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpForce);
        //    _jumpCount--;
        //}
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
            else if (transform.position.x < other.gameObject.transform.position.x)
            {
                _isHurt = true;
                _rigidbody2D.velocity = new Vector2(-1 * _hurtForce, _hurtForce);
            }
            else if (transform.position.x > other.gameObject.transform.position.x)
            {
                _isHurt = true;
                _rigidbody2D.velocity = new Vector2(_hurtForce, _hurtForce);
            }
        }
    }

    void SwitchAnimation()
    {
        _animator.SetFloat("running", Mathf.Abs(_running));
        //
        _animator.SetBool("isCrouch", _isCrouch);
        //
        _animator.SetBool("isJump", !_isGrounded);
        _animator.SetBool("isFall", false);
        if (!_isGrounded && _rigidbody2D.velocity.y <= 0)
        {
            _animator.SetBool("isJump", false);
            _animator.SetBool("isFall", true);
        }
        //
        if (_isLanded)
        {
            _animator.SetTrigger("triLanded");
        }
        //
        _animator.SetBool("isHurt", _isHurt);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_isGrounded) { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        //
        Gizmos.color = Color.red;
        if (_canStand) { Gizmos.color = Color.green; }
        Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
    }
}
