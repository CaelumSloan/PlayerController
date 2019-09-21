using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityCharacterController : MonoBehaviour
{
    void Start()
    {
        
    }

    float verticalVelocity;

    void FixedUpdate()
    {
        verticalVelocity += -9.81f * Time.fixedDeltaTime;
    }
}
