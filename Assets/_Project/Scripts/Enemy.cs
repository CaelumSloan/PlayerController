using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    void Update()
    {
        Quaternion lookOnLook =
        Quaternion.LookRotation(PlayerRefrenceHolder.Instance.transform.position - transform.position);

        transform.rotation =
        Quaternion.Slerp(transform.rotation, lookOnLook, Time.deltaTime);

        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }
}
