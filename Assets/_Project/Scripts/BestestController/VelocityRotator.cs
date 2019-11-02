using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityRotator : MonoBehaviour
{
    [SerializeField] private NextPositionFinder player;

    void Update()
    {
        if (player.PlayerVelocity.magnitude > Mathf.Epsilon)
        {
            var cross = Vector3.Cross(transform.InverseTransformDirection(player.PlayerVelocity.normalized), Vector3.up);
            transform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.AngleAxis(-7.5f, cross), Mathf.Clamp01(player.PlayerVelocity.magnitude / (1.5f * player.MaxSpeed)));
        }
        else
            transform.localRotation = Quaternion.identity;
    }
}
