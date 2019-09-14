using BulletSharp;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BRigidBody))]
public class BlastJump : MonoBehaviour
{
    private BRigidBody rigidBody;

    [SerializeField] private Transform startRaycastFrom;

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
    [Range(0, 7f)]
    [SerializeField] private float timeBetweenShots = 1;

    private bool blastJumpToken;
    private bool handledThisFrame;
    [Space(5)]
    [SerializeField] private bool enableDebugMode;

    private Timer timer;

    void Start()
    {
        timer = Timer.CreateTimer(timeBetweenShots);
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
        blastJumpToken = false;
        handledThisFrame = true;

        if (!timer.CheckAndReset()) return;

        BulletSharp.Math.Vector3 from = startRaycastFrom.position.ToBullet();
        BulletSharp.Math.Vector3 to = (startRaycastFrom.position + startRaycastFrom.forward * maxBlastJumpRange).ToBullet();

        ClosestRayResultCallback callback = new ClosestRayResultCallback(ref from, ref to);
        BPhysicsWorld.Get().world.RayTest(from, to, callback);

        if (enableDebugMode)
            Debug.DrawLine(from.ToUnity(), to.ToUnity(), Color.green, 2f);

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
        }
    }
}