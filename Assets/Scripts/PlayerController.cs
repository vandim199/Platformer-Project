using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public TMP_Text debugText;
    public float coyoteTimeLength;
    public Collider2D leftCollider;
    public Collider2D rightCollider;
    public float skewLimiter;
    public float deltaSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float decceleration;
    [SerializeField] private float velocityScale;
    [SerializeField] private JumpCurve jumpCurve;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float jumpBufferTimer = 0.5f;
    [SerializeField] private float groundedLength = 2;
    [SerializeField] private float floorFriction;
    [SerializeField] private LayerMask layerMask;
    
    private Rigidbody2D _rb;
    private bool _grounded = false;
    public bool GetIsGrounded
    {
        get { return _grounded;}
        
    }

    private float _jumpBufferedTime = -100;
    private float _coyoteTimer = 999999999;
    private float _timestamp = 999999999;

    private float _targetX, _targetY;

    private Vector2 _moveDir;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
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
           //_rb.velocity = _moveDir;
        }
        if (Input.GetButtonDown("Jump"))
        {
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
    }

    void Jump()
    {
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
        
        if(leftCollider.IsTouchingLayers(layerMask) && _moveDir.x < 0)
        {
            //_rb.velocity = new Vector2(0, _rb.velocity.y);
        }
        if(rightCollider.IsTouchingLayers(layerMask) && _moveDir.x > 0)
        {
            //_rb.velocity = new Vector2(0, _rb.velocity.y);
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
        
        _rb.AddForce(movement * Vector2.right);
    }

    void Friction()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.05f)
        {
            Debug.Log(true);
            float amount = Mathf.Min(Mathf.Abs(_rb.velocity.x), Mathf.Abs(floorFriction));
            amount *= Mathf.Sign(_rb.velocity.x);
            _rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        else Debug.Log(false);
    }

    IEnumerator BufferTimeOut()
    {
        yield return new WaitForSeconds(jumpBufferTimer);
        _jumpBufferedTime = 0;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("JumpPoint"))
        {
            _coyoteTimer = coyoteTimeLength;
        }
    }
}
