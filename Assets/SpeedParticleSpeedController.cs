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
        main.startSpeed = Mathf.Floor(quickVel.magnitude / PlayerRefrenceHolder.Instance.nextPositionFinder.MaxSpeed) * 35f;
    }
}
