using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float runSpeed = 3;
    [SerializeField]
    private float jumpForce = 20;
    [SerializeField] 
    private AnimationCurve jumpCurve;
    [SerializeField]
    private float jumpBufferTimer = 0.5f;
    [SerializeField]
    private float groundedLength = 2;
    [SerializeField]
    private LayerMask layerMask;
    
    private Rigidbody2D _rb;
    private bool _grounded = false;
    private bool _jumpBuffered = false;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            _rb.velocity = new Vector2(Input.GetAxis("Horizontal") * runSpeed, _rb.velocity.y);
        }
        if (Input.GetButtonDown("Jump"))
        {
            if(_grounded)
            {
                Jump();
            }
            else
            {
                _jumpBuffered = true;
                StartCoroutine(bufferTimeOut());
            }
            
        }
    }

    void Jump()
    {
        Vector2 vec = _rb.velocity;
        vec.y = jumpForce;
        _rb.velocity = vec;
        _jumpBuffered = false;
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
    }

    IEnumerator bufferTimeOut()
    {
        yield return new WaitForSeconds(jumpBufferTimer);
        _jumpBuffered = false;
    }
}
