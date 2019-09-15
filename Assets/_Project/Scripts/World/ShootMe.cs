using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootMe : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
            GetComponent<BRigidBody>().AddImpulse((transform.forward + transform.up * 0.5f).normalized * 500);
    }
}
