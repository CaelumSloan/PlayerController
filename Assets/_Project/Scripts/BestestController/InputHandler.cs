using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This could really go anywhere, doesn't even have to be a 
/// MonoBehaviour I think.
/// </summary>
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
            wishDir += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            wishDir += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            wishDir += Vector3.back;
        }
        if (Input.GetKey(KeyCode.D))
        {
            wishDir += Vector3.right;
        }
    }

    //Jump register logic, etc.
}
