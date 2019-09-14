using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletUnity;

[RequireComponent(typeof(BRigidBody))]
public class PlayerController : MonoBehaviour
{
    private BRigidBody rigidBody;

    [SerializeField] private float moveSpeed;
    [Range(0,2)]
    [SerializeField] private float airFriction;
    GroundedChecker groundedChecker;

    private void Start()
    {
        rigidBody = GetComponent<BRigidBody>();
        groundedChecker = GetComponent<GroundedChecker>();
    }

    void FixedUpdate()
    {
        Vector3 moveDir = GetMoveDir() * moveSpeed;
        Vector3 currentVelocity = rigidBody.velocity;

        //Amount of requested direction in line with current direction
        Vector3 agreenceComponent = Mathf.Abs(moveDir.ScalarProjection(currentVelocity)) * currentVelocity;
        Vector3 disAgreenceComponent = moveDir - agreenceComponent;
        Vector3 influencedVelocity = currentVelocity + disAgreenceComponent;

        bool velocityBiggerThanMoveDir = currentVelocity.magnitude > moveDir.magnitude;
        float biggestMag = velocityBiggerThanMoveDir ? currentVelocity.magnitude : moveDir.magnitude;
        float scaling = velocityBiggerThanMoveDir ? Mathf.Min(influencedVelocity.magnitude, biggestMag) : moveDir.magnitude;

        Vector3 newVelocity = influencedVelocity.normalized * scaling;

        float airFrictionVal = groundedChecker != null && !groundedChecker.IsGrounded() ? airFriction : 1;

        rigidBody.velocity = new Vector3(newVelocity.x * airFrictionVal, currentVelocity.y, newVelocity.z * airFrictionVal);
    }

    private Vector3 GetMoveDir()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDir += transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir += -transform.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir += -transform.forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += transform.right;
        }

        return moveDir.normalized;
    }
}
