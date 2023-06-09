using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public TMP_Text debugText;
    public float coyoteTime;
    public Collider2D leftCollider;
    public Collider2D rightCollider;
    public float skewLimiter;
    public float deltaSpeed;
    [SerializeField]
    private JumpCurve jumpCurve;
    [SerializeField]
    private float runSpeed = 3;
    [SerializeField]
    private float jumpBufferTimer = 0.5f;
    [SerializeField]
    private float groundedLength = 2;
    [SerializeField]
    private LayerMask layerMask;
    
    private Rigidbody2D _rb;
    private bool _grounded = false;
    private bool _jumpBuffered = false;

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
            _coyoteTimer = coyoteTime;
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
           _rb.velocity = _moveDir;
        }
        if (Input.GetButtonDown("Jump"))
        {
            if(_coyoteTimer > 0)
            {
                Jump();
            }
            else
            {
                _jumpBuffered = true;
                StartCoroutine(bufferTimeOut());
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
        _jumpBuffered = false;
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
        RaycastHit2D hit;
        hit = Physics2D.Raycast(transform.position, Vector2.down, groundedLength, layerMask);
        if (hit.collider != null)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.down * groundedLength, Color.green);
            _grounded = true;
            if (_jumpBuffered)
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
        
        RaycastHit2D hitLeft;
        hitLeft = Physics2D.Raycast(transform.position, _moveDir.normalized, groundedLength, layerMask);
        //if (hitLeft.collider != null)
        if(leftCollider.IsTouchingLayers(layerMask) && _moveDir.x < 0)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }
        if(rightCollider.IsTouchingLayers(layerMask) && _moveDir.x > 0)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }
    }

    IEnumerator bufferTimeOut()
    {
        yield return new WaitForSeconds(jumpBufferTimer);
        _jumpBuffered = false;
    }
}
