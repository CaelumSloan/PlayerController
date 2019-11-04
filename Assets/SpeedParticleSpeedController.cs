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
        var quickVel = PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity;
        quickVel.y = 0;
        main.startSpeed = (quickVel.magnitude / PlayerRefrenceHolder.Instance.nextPositionFinder.MaxSpeed)*35f;
    }
}
