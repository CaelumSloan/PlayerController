using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSMouseLook : MonoBehaviour
{
    [SerializeField] private GameObject camVertRot;

    private Vector2 rotation = new Vector2(0, 0);
    [SerializeField] private float rotationSpeed = 10;


    void Update()
    {
        rotation.x += -Input.GetAxis("Mouse Y");
        camVertRot.transform.eulerAngles = new Vector2(rotation.x * rotationSpeed, transform.eulerAngles.y);
        rotation.y += Input.GetAxis("Mouse X");
        transform.eulerAngles = new Vector2(0, rotation.y) * rotationSpeed;
    }
}
