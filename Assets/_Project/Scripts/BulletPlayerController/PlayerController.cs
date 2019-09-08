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

        moveDir = moveDir.normalized;

        rigidBody.AddForce(Vector3.ClampMagnitude(moveDir * moveSpeed, maxSpeed));
    }
}
