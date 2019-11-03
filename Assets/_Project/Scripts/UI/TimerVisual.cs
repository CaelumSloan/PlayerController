using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerVisual : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI textMeshPro;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = Mathf.RoundToInt(TimeManager.Instance.timer.TimeRemaining()).ToString();
    }
}
