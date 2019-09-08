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

    private bool jumpToken;
    private bool handledThisFrame;

    void Start()
    {
        jumpToken = false;
        rigidBody = GetComponent<BRigidBody>();
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
        rigidBody.AddImpulse(Vector3.up * jumpForce);
        jumpSFX.Stop();
        jumpSFX.Play();
        jumpToken = false;
        handledThisFrame = true;
    }

    private void CheckForInput()
    {
        if (!jumpToken && !handledThisFrame)
            jumpToken = (Input.GetKeyDown(KeyCode.Space));
    }
}
