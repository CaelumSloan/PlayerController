using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSMouseLook : MonoBehaviour
{
    [SerializeField] private GameObject camVertRot;

    private Vector2 rotation = new Vector2(0, 0);
    [SerializeField] private float rotationSpeed = 10;
    [Range(30,112)]
    [SerializeField] private float rotationClamp = 80;

    void Update()
    {
        rotation.x += -Input.GetAxis("Mouse Y") * rotationSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -rotationClamp, rotationClamp);
        camVertRot.transform.eulerAngles = new Vector2(rotation.x , transform.eulerAngles.y);
        rotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
        transform.eulerAngles = new Vector2(0, rotation.y);
    }
}
