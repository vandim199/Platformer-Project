using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public TMP_Text debugText;
    public float coyoteTimeLength;
    public float skewLimiter;
    public float deltaSpeed;
    public JumpCurve[] curvesArray;
    public TMP_Text text;
    
    [SerializeField] private float acceleration;
    [SerializeField] private float decceleration;
    [SerializeField] private float velocityScale;
    [SerializeField] private JumpCurve jumpCurve;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float dashLength = 5;
    [SerializeField] private float jumpBufferTimer = 0.5f;
    [SerializeField] private float groundedLength = 2;
    [SerializeField] private float floorFriction;
    [SerializeField] private LayerMask layerMask;

    public bool GetIsGrounded
    {
        get { return _grounded;}
    }
    
    private int _curveNum = 0;
    private Animator _anim;
    private Rigidbody2D _rb;
    private bool _grounded = false;
    private float _jumpBufferedTime = -100;
    private float _coyoteTimer = 999999999;
    private float _timestamp = 999999999;
    private float _targetX, _targetY;
    private Vector2 _moveDir;
    private bool _canDoubleJump;
    private GameObject _lastJumpPoint;
    // Start is called before the first frame update

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_grounded)
        {
            _coyoteTimer = coyoteTimeLength;
            debugText.text = "jump ok";
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
            if(_coyoteTimer <= 0) debugText.text = "no";
        }
        
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
           _moveDir = new Vector2(Input.GetAxis("Horizontal") * runSpeed, _rb.velocity.y);
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (_canDoubleJump)
            {
                _coyoteTimer = coyoteTimeLength;
                _canDoubleJump = false;
                ParticleSystem particle = _lastJumpPoint.GetComponentInChildren<ParticleSystem>();
                particle.Clear();
                particle.Play();
            }
            if(_coyoteTimer > 0)
            {
                Jump();
            }
            else
            {
                _jumpBufferedTime = Time.time;
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            _coyoteTimer = 0;
            if (_rb.velocity.y > 0)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, -jumpCurve.gravityOnRelease);
            }
        }
        
        ShearAndTear();
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            _curveNum++;
            _curveNum = (int)Mathf.Repeat(_curveNum, curvesArray.Length);
            jumpCurve = curvesArray[_curveNum];
            text.text = "T - Change jump feel\n" + jumpCurve.name;
        }
    }

    void Jump()
    {
        _anim.SetTrigger("Jump");
        Vector2 vec = _rb.velocity;
        vec.y = jumpCurve.jumpForce;
        _rb.velocity = vec;
        _timestamp = Time.time;
        _jumpBufferedTime = 0;
    }

    void ShearAndTear()
    {
        _targetX = _rb.velocity.y != 0 ? 0 : Mathf.Clamp(-_rb.velocity.x * 0.04f, -skewLimiter, skewLimiter);
        _targetY = _rb.velocity.y == 0 ? 0 : Mathf.Clamp(_rb.velocity.x * 0.04f, -skewLimiter, skewLimiter);
        
        _targetY = Mathf.Clamp(Mathf.Abs(_rb.velocity.y * 0.04f), 0, skewLimiter);
        _targetX = Mathf.Clamp(Mathf.Abs(_rb.velocity.x * 0.04f), 0, skewLimiter) - _targetY;

        
        float currentX = Mathf.Lerp(gameObject.GetComponent<SpriteRenderer>().material.GetFloat("_xSkew"), _targetX, Time.deltaTime * deltaSpeed);
        float currentY = Mathf.Lerp(gameObject.GetComponent<SpriteRenderer>().material.GetFloat("_ySkew"), _targetY, Time.deltaTime * deltaSpeed);
        
        gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_xSkew", currentX);
        gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_ySkew", currentY);
    }

    private void FixedUpdate()
    {
        RaycastHit2D groundRaycast;
        groundRaycast = Physics2D.Raycast(transform.position, Vector2.down, groundedLength, layerMask);
        if (groundRaycast.collider != null)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.down * groundedLength, Color.green);
            _grounded = true;
            if (_jumpBufferedTime + jumpBufferTimer > Time.time)
            {
                Jump();
            }
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.down * groundedLength, Color.red);
            _grounded = false;
        }

        if (!_grounded)
        {
            Vector2 vec = _rb.velocity;
            if (_rb.velocity.y > 0)
            {
                vec.y -= jumpCurve.riseGravity.Evaluate(Time.time - _timestamp);
            }
            else if (_rb.velocity.y < 0)
            {
                vec.y -= jumpCurve.fallGravity.Evaluate(Time.time - _timestamp);
            }

            _rb.velocity = vec;
        }
        
        
        Run();
        Friction();
    }

    void Run()
    {
        float targetSpeed = runSpeed * Input.GetAxis("Horizontal");
        float speedDiff = targetSpeed - _rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velocityScale) * Mathf.Sign(speedDiff);
        
        _anim.SetFloat("Speed", targetSpeed);
        _rb.AddForce(movement * Vector2.right);
    }

    void Friction()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.05f)
        {
            float amount = Mathf.Min(Mathf.Abs(_rb.velocity.x), Mathf.Abs(floorFriction));
            amount *= Mathf.Sign(_rb.velocity.x);
            _rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    IEnumerator BufferTimeOut()
    {
        yield return new WaitForSeconds(jumpBufferTimer);
        _jumpBufferedTime = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("JumpPoint"))
        {
            _canDoubleJump = true;
            _lastJumpPoint = other.gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("JumpPoint"))
        {
            _canDoubleJump = false;
        }
    }
}
