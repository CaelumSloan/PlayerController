using BulletSharp;
using BulletUnity;
using BulletUnity.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedChecker : MonoBehaviour
{
    private BCapsuleShape capsule;

    public bool enableDebugMode = false;
    [Range(0,1)]
    [SerializeField] private float heightExtension = 0.1f;
    [Range(0, 2)]
    [SerializeField] private float whiskerDist = 1.75f;

    private bool isGrounded = true;

    public bool IsGrounded()
    {
        return isGrounded;
    }

    void Start()
    {
        capsule = GetComponent<BCapsuleShape>();
    }

    void FixedUpdate()
    {
        BulletSharp.Math.Vector3 from = transform.position.ToBullet();

        List<bool> hits = new List<bool>() {false, false, false, false, false };

        //float theta = angle * Mathf.PI;
        //float sin = Mathf.Sin(theta);
        //float cos = Mathf.Cos(theta);
        //float tan = Mathf.Tan(theta);

        var endPos = transform.position + (Vector3.down * (capsule.Height + heightExtension));

        for (int whiskerNum = 0; whiskerNum < 5; whiskerNum++)
        {
            BulletSharp.Math.Vector3 to;
            switch (whiskerNum)
            {
                case 0:
                    to = endPos.ToBullet();
                    break;
                case 1:
                    to = (endPos + (Vector3.forward * whiskerDist)).ToBullet();
                    break;
                case 2:
                     to = (endPos + (Vector3.back * whiskerDist)).ToBullet();
                    break;
                case 3:
                     to = (endPos + (Vector3.left * whiskerDist)).ToBullet();
                    break;
                case 4:
                    to = (endPos + (Vector3.right * whiskerDist)).ToBullet();
                    break;
                default:
                    to = endPos.ToBullet();
                    break;
            }

            ClosestRayResultCallback callback = new ClosestRayResultCallback(ref from, ref to);
            BPhysicsWorld.Get().world.RayTest(from, to, callback);

            hits[whiskerNum] = callback.HasHit;

            Debug.DrawLine(from.ToUnity(), to.ToUnity(), callback.HasHit ? Color.green : Color.red);
        }

        isGrounded = hits.Contains(true);
    }
}
