using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MatchCapsuleScale : MonoBehaviour
{
    [SerializeField] private BCapsuleShape bulletCapsule;

    void Update()
    {
        if (!bulletCapsule) return;

        Vector3 upAxis = GetUpAxis();
        Vector3 radiusAxis = (Vector3.one - upAxis);

        var height = bulletCapsule.Height;
        var diameter = bulletCapsule.Radius * 2;

        transform.localScale = upAxis * height + radiusAxis * diameter;
    }

    Vector3 GetUpAxis()
    {
        switch (bulletCapsule.UpAxis)
        {
            case BulletUnity.BCapsuleShape.CapsuleAxis.x:
                return bulletCapsule.transform.right;
            case BulletUnity.BCapsuleShape.CapsuleAxis.y:
                return bulletCapsule.transform.up;
            case BulletUnity.BCapsuleShape.CapsuleAxis.z:
                return bulletCapsule.transform.forward;
            default: return Vector3.zero;
        }
    }
}
