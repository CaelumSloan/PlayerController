using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletUnity;

[RequireComponent(typeof(BRigidBody))]
public class PlayerController : MonoBehaviour
{
    private BRigidBody rigidBody;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;

    private void Start()
    {
        rigidBody = GetComponent<BRigidBody>();
    }

    void FixedUpdate()
    {
        Vector3 moveDir = GetMoveDir();
        Vector3 currentVelocity = rigidBody.velocity;

        //Amount of requested direction in line with current direction
        Vector3 agreenceComponent = Mathf.Abs(moveDir.ScalarProjection(currentVelocity)) * currentVelocity;
        Vector3 disAgreenceComponent = moveDir - agreenceComponent;
        Vector3 influencedVelocity = currentVelocity + disAgreenceComponent;

        bool velocityBiggerThanMoveDir = currentVelocity.magnitude > moveDir.magnitude;
        float biggestMag = velocityBiggerThanMoveDir ? currentVelocity.magnitude : moveDir.magnitude;
        float scaling = velocityBiggerThanMoveDir ? Mathf.Min(influencedVelocity.magnitude, biggestMag) : moveDir.magnitude;

        Vector3 newVelocity = influencedVelocity.normalized * scaling;
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
