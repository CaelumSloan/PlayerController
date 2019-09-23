using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    Vector3 wishDir;

    public Vector3 GetWishDir()
    {
        return wishDir.normalized;
    }

    void Update()
    {
        wishDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            wishDir += transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            wishDir += -transform.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            wishDir += -transform.forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            wishDir += transform.right;
        }
    }

    //Jump register logic, etc.
}
