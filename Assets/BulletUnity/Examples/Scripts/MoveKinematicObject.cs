using UnityEngine;
using System.Collections;
using BulletUnity;

public class MoveKinematicObject : MonoBehaviour {

    Vector3 startPos;
    [Range(0,2)]
    public float animStrength = .5f;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    void FixedUpdate()
    {
        transform.localPosition = transform.localPosition + new UnityEngine.Vector3(Mathf.Sin(Time.time),0f,Mathf.Cos(Time.time)) * animStrength;
    }
}
