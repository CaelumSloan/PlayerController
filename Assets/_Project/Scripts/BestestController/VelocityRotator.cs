using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VelocityRotator : MonoBehaviour
{
    [SerializeField] private NextPositionFinder player;
    [SerializeField] private new Camera camera;

    float acceleration = 0;
    [SerializeField] private float rotationDegrees = 7.5f;
    [SerializeField] private float accelCausingMaxEffect = 350f;

    void Update()
    {
        if (player.PlayerVelocity.magnitude > Mathf.Epsilon)
        {
            var cross = Vector3.Cross(transform.InverseTransformDirection(player.PlayerVelocity.normalized), Vector3.up);
            var normAngle = 1 - Vector3.Angle(camera.transform.forward, PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity) / 180;
            transform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.AngleAxis(-rotationDegrees, cross), Mathf.Clamp01(player.PlayerVelocity.magnitude / player.MaxSpeed) * normAngle * Mathf.Clamp01(acceleration/accelCausingMaxEffect));
        }
        else
            transform.localRotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        acceleration = AccelerationFinder();
    }

    float lastVel=0;
    float AccelerationFinder()
    {
        var vel = player.PlayerVelocity;
        vel.y = 0;
        var accel = Mathf.Abs(lastVel - vel.magnitude) / Time.deltaTime;
        lastVel = vel.magnitude;
        return accel;
    }
}
