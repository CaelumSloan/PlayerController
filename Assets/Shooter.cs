using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) Shoot();
    }

    void Shoot()
    {
        var gj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var rb = gj.AddComponent<Rigidbody>();
        rb.AddForce(20f * transform.forward + PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity);
    }
}
