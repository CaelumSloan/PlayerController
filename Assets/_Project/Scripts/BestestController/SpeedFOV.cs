using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedFOV : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private float fovEffectStrength = 10;
    [SerializeField] private AnimationCurve modifier;

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

        var v1 = PlayerRefrenceHolder.Instance.nextPositionFinder.PlayerVelocity;
        v1.y = 0;
        v1 = v1.normalized;
        var v2 = PlayerRefrenceHolder.Instance.gameObjectCamera.transform.forward;
        v2.y = 0;
        v2 = v2.normalized;
        var dot = Mathf.Clamp01(Vector3.Dot(v1, v2));

        float t = Mathf.Clamp01(speed / PlayerRefrenceHolder.Instance.nextPositionFinder.MaxSpeed) * dot;
        t = modifier.Evaluate(t);
        float fov = Mathf.Lerp(startFov, startFov + fovEffectStrength, t*normAngle);
        camera.fieldOfView = fov;
    }
}
