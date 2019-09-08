using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BRigidBody))]
public class FPSMouseLook : MonoBehaviour
{
    private BRigidBody rigidBody;

    [SerializeField] private GameObject camVertRot;

    private Vector2 rotation = new Vector2(0, 0);
    [SerializeField] private float rotationSpeed = 10;

    private void Start()
    {
        //fps cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rigidBody = GetComponent<BRigidBody>();
    }

    void Update()
    {
        rotation.x += -Input.GetAxis("Mouse Y");
        camVertRot.transform.eulerAngles = new Vector2(rotation.x * rotationSpeed, transform.eulerAngles.y);
        rotation.y += Input.GetAxis("Mouse X");
        rigidBody.SetRotation(Quaternion.Euler(new Vector2(0, rotation.y) * rotationSpeed));
        rigidBody.angularVelocity = Vector3.zero;
    }
}
