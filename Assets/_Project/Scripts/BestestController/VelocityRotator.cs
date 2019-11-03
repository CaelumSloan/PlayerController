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
    Queue<float> lastSpeeds = new Queue<float>(5);

    void Update()
    {
        if (player.PlayerVelocity.magnitude > Mathf.Epsilon)
        {
            var cross = Vector3.Cross(transform.InverseTransformDirection(player.PlayerVelocity.normalized), Vector3.up);
            var normAngle = 1 - Vector3.Angle(camera.transform.forward, PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity) / 180;
            transform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.AngleAxis(-7.5f, cross), Mathf.Clamp01(player.PlayerVelocity.magnitude / (1.5f * player.MaxSpeed)) * normAngle * Mathf.Clamp01(acceleration/350f));
        }
        else
            transform.localRotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        acceleration = AccelerationFinder();
    }

    float i=0;
    float AccelerationFinder()
    {
        float last=0;
        if (i < 6) i++;
        else
            last=lastSpeeds.Dequeue();

        var vel = player.PlayerVelocity;
        vel.y = 0;
        lastSpeeds.Enqueue(vel.magnitude);

        return Mathf.Abs(last-vel.magnitude) / (Time.deltaTime * lastSpeeds.Count);
    }
}
