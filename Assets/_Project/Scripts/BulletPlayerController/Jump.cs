using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BRigidBody))]
public class Jump : MonoBehaviour
{
    private BRigidBody rigidBody;

    [SerializeField] private float jumpForce = 10;
    [SerializeField] private AudioSource jumpSFX;
    GroundedChecker groundedChecker;

    private bool jumpToken;
    private bool handledThisFrame;

    void Start()
    {
        jumpToken = false;
        rigidBody = GetComponent<BRigidBody>();
        groundedChecker = GetComponent<GroundedChecker>();
    }

    void Update()
    {
        CheckForInput();
        handledThisFrame = false;
    }

    private void FixedUpdate()
    {
        CheckForInput();

        if (jumpToken)
            ExecuteJump();
    }

    private void ExecuteJump()
    {
        jumpToken = false;
        handledThisFrame = true;

        if (groundedChecker != null)
            if (!groundedChecker.IsGrounded())
                return;

        rigidBody.AddImpulse(Vector3.up * jumpForce);
        jumpSFX.Stop();
        jumpSFX.Play();
    }

    private void CheckForInput()
    {
        if (!jumpToken && !handledThisFrame)
            jumpToken = (Input.GetKeyDown(KeyCode.Space));
    }
}
