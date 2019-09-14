using BulletSharp;
using BulletUnity;
using BulletUnity.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BCapsule))]
public class GroundedChecker : MonoBehaviour
{
    private BCapsuleShape capsule;
    private BRigidBody rigidbody;

    public bool enableDebugMode = false;
    [Range(0,1)]
    [SerializeField] private float heightExtension = 0.1f;

    private bool isGrounded = true;

    public bool IsGrounded()
    {
        return isGrounded;
    }

    void Start()
    {
        capsule = GetComponent<BCapsuleShape>();
        rigidbody = GetComponent<BRigidBody>();
    }

    void FixedUpdate()
    {
        BulletSharp.Math.Vector3 from = transform.position.ToBullet();
        BulletSharp.Math.Vector3 to = (transform.position + (Vector3.down * (capsule.Height + heightExtension))).ToBullet();

        ClosestRayResultCallback callback = new ClosestRayResultCallback(ref from, ref to);
        BPhysicsWorld.Get().world.RayTest(from, to, callback);

        isGrounded = callback.HasHit || Mathf.Approximately(rigidbody.velocity.y,0);

        if (enableDebugMode)
            Debug.DrawLine(from.ToUnity(), to.ToUnity(), isGrounded ? Color.green : Color.red);
    }
}
