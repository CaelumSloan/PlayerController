using BulletSharp;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BRigidBody))]
public class BlastJump : MonoBehaviour
{
    private BRigidBody rigidBody;

    [Range(1, 7f)]
    [SerializeField] private float blastJumpForce = 1;
    [Range(0, 20)]
    [SerializeField] private float maxBlastJumpRange;
    [Range(0, 1f)]
    [SerializeField] private float smallestBlastForce;
    [Range(0, 1)]
    [SerializeField] private float maxBlastJumpForce;
    [Range(0, .75f)]
    [SerializeField] private float falloffStrength = .27f;

    private bool blastJumpToken;
    private bool handledThisFrame;
    [Space(5)]
    [SerializeField] private bool enableDebugMode;

    void Start()
    {
        blastJumpToken = false;
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

        if (blastJumpToken)
            ExecuteBlastJump();
    }

    private void CheckForInput()
    {
        if (!blastJumpToken && !handledThisFrame)
            blastJumpToken = (Input.GetMouseButtonDown(1));
    }

    public void ExecuteBlastJump()
    {
        Transform startTransform = Camera.main.transform;
        BulletSharp.Math.Vector3 from = startTransform.position.ToBullet();
        BulletSharp.Math.Vector3 to = (startTransform.position + startTransform.forward * maxBlastJumpRange).ToBullet();

        ClosestRayResultCallback callback = new ClosestRayResultCallback(ref from, ref to);
        BPhysicsWorld.Get().world.RayTest(from, to, callback);

        if (enableDebugMode)
            Debug.DrawRay(from.ToUnity(), to.ToUnity(), Color.green, 2f);

        if (callback.HasHit)
        {
            float force = 0;

            Vector3 meToGround = callback.HitPointWorld.ToUnity() - transform.position;
            float dist = meToGround.magnitude;
            if (dist < maxBlastJumpRange)
            {
                //Falloff curve visual: https://www.desmos.com/calculator/plvrrosegp
                var b = 1f / (Mathf.Pow(maxBlastJumpRange, 2) * smallestBlastForce);
                force = blastJumpForce / (1 * maxBlastJumpForce + (falloffStrength * Mathf.Abs(dist)) + (b * Mathf.Pow(Mathf.Abs(dist), 2)));
            }

            rigidBody.AddImpulse(-meToGround.normalized * force);

            #region Debug
            if (enableDebugMode)
            {
                Debug.Log("Blasting off with a force of: " + force);
                var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                s.transform.localScale = Vector3.one * .1f;
                s.transform.position = callback.HitPointWorld.ToUnity();
                s.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            #endregion

            blastJumpToken = false;
            handledThisFrame = true;
        }
    }
}