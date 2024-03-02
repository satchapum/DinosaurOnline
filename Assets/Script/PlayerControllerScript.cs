using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerScript : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    public float jumpForce = 5f;

    public bool isGrounded;
    public Collider groundGroupCollider;

    private Animator animator;
    private Rigidbody rb;
    private bool running;

    // Start is called before the first frame update
    void Start()
    {
        GameObject groundGroup = GameObject.Find("GroundGroup");
        groundGroupCollider = groundGroup.GetComponent<Collider>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        isGrounded = true;
        running = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == groundGroupCollider)
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider == groundGroupCollider)
        {
            isGrounded = false;
        }
    }

    void moveForward()
    {
        float verticalInput = Input.GetAxis("Horizontal");

        float translation = verticalInput * speed;
        translation *= Time.fixedDeltaTime;
        rb.MovePosition(rb.position + this.transform.forward * translation);

        if (!running)
        {
            running = true;
            animator.SetBool("Running", true);
        }

        else if (running)
        {
            running = false;
            animator.SetBool("Running", false);
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            animator.SetBool("Jump", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
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
    }
}
