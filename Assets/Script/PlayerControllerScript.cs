using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerScript : NetworkBehaviour
{
    public float speed = 5.0f;
    float temp_Speed;
    public float rotationSpeed = 10.0f;
    public float jumpForce = 5f;
    public float speedWhenSlow = 1.0f;

    public bool isGrounded;
    public Collider groundGroupCollider;

    private Animator animator;
    private Rigidbody rb;
    private bool running;

    [SerializeField] Collider normalCollider;
    [SerializeField] Collider halfCollider;

    // Start is called before the first frame update
    void Start()
    {
        temp_Speed = speed;
        GameObject groundGroup = GameObject.Find("GroundGroup");
        groundGroupCollider = groundGroup.GetComponent<Collider>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        isGrounded = true;
        running = false;
        halfCollider.enabled = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == groundGroupCollider)
        {
            isGrounded = true;
            //animator.SetBool("Jump", false);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider == groundGroupCollider)
        {
            isGrounded = false;
        }
    }
    public void playerGetDelay()
    {
        speed = speedWhenSlow;
    }
    public void playerDontGetDelay()
    {
        speed = temp_Speed;
    }
    [ClientRpc]
    public void playerGetDelayClientRpc()
    {
        speed = speedWhenSlow;
    }
    [ClientRpc]
    public void playerDontGetDelayClientRpc()
    {
        speed = temp_Speed;
    }


    void moveForward()
    {
        float zInput = Input.GetAxis("Horizontal");

        Vector3 newPosition = transform.position + Vector3.right * zInput * speed * Time.deltaTime;

        transform.position = newPosition;

        if (!running)
        {
            running = true;
            //animator.SetBool("Running", true);
        }

        else if (running)
        {
            running = false;
            //animator.SetBool("Running", false);
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            //animator.SetBool("Jump", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    void Crouch()
    {
        normalCollider.enabled = false;
        halfCollider.enabled = true;
    }

    private void FixedUpdate()
    {
        moveForward();
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Crouch();
            return;
        }

        normalCollider.enabled = true;
        halfCollider.enabled = false;

    }
}
