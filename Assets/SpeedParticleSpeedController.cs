using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedParticleSpeedController : MonoBehaviour
{
    [SerializeField] private new ParticleSystem particleSystem;

    ParticleSystem.MainModule main;

    void Start()
    {
        main = particleSystem.main;
    }

    void Update()
    {
        //main.startSpeed = PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity.magnitude==PlayerRefrenceHolder.Instance.nextPositionFinder.MaxSpeed ? 35f : 0f;
        var quickVel = PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity;
        quickVel.y = 0;
        var v1 = PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity;
        v1.y = 0;
        v1 = v1.normalized;
        var v2 = PlayerRefrenceHolder.Instance.gameObjectCamera.transform.forward;
        v2.y = 0;
        v2 = v2.normalized;
        var on = Vector3.Dot(v1, v2) > .96f ? Mathf.Floor(quickVel.magnitude / PlayerRefrenceHolder.Instance.nextPositionFinder.MaxSpeed) * 35f : 0;
        main.startSpeed = on;
    }
}
