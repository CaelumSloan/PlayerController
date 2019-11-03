using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VelocityVisual : MonoBehaviour
{
    [SerializeField] NextPositionFinder playerLogic;
    [SerializeField] Text velocityText;

    private float lastSpeed = Mathf.NegativeInfinity;
    private float currentSpeed = 0;

    void Update()
    {
        Vector3 copyOfVelocity = playerLogic.PlayerVelocity;
        copyOfVelocity.y = 0;
        currentSpeed = Mathf.RoundToInt(copyOfVelocity.magnitude*100);
        velocityText.text = currentSpeed.ToString();

        if (currentSpeed > lastSpeed) velocityText.color = Color.green;
        else if (currentSpeed < lastSpeed) velocityText.color = Color.red;
        else if (currentSpeed == 1400) velocityText.color = Color.blue;
        else if (currentSpeed != 1750) velocityText.color = Color.white;
        else velocityText.color = Color.yellow;

        lastSpeed = currentSpeed;
    }
}
