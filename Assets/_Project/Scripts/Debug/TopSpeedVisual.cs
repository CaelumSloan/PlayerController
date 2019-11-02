using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopSpeedVisual : MonoBehaviour
{
    [SerializeField] NextPositionFinder playerLogic;
    [SerializeField] Text topSpeedText;

    private float currentSpeed;

    private float topSpeed = Mathf.NegativeInfinity;

    void Update()
    {
        Vector3 copyOfVelocity = playerLogic.PlayerVelocity;
        copyOfVelocity.y = 0;
        currentSpeed = Mathf.RoundToInt(copyOfVelocity.magnitude*100);
        topSpeedText.text = topSpeed.ToString();

        topSpeed = Mathf.Max(currentSpeed, topSpeed);
    }
}
