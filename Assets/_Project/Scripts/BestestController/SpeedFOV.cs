using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedFOV : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private float fovEffectStrength = 10;

    private float startFov = 90;

    private void Awake()
    {
        startFov = camera.fieldOfView;
    }

    void Update()
    {

        var vel = PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity;
        vel.y = 0;
        var speed= vel.magnitude;

        var normAngle = 1-Vector3.Angle(camera.transform.forward, PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity) / 180;
        float t = Mathf.Clamp01(speed / PlayerRefrenceHolder.Instance.nextPositionFinder.MaxSpeed);
        float fov = Mathf.Lerp(startFov, startFov + fovEffectStrength, t*normAngle);
        camera.fieldOfView = fov;
    }
}
