using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rewarder : MonoBehaviour
{
    public float oneInEveryJumps = 4;

    public List<AudioClip> rewards = new List<AudioClip>();
    public GameObject reward;

    AudioSource adsc;

    bool startBool = true;

    private void Start()
    {
        adsc = GetComponent<AudioSource>();
    }

    public void Reward()
    {
        if (startBool)
        {
            startBool = false;
            oneInEveryJumps += .1f;
        }
        else
        {
            if (Random.Range(0, (int)Mathf.Floor(oneInEveryJumps+=(.1f/oneInEveryJumps))) != 0) return;
        }

        if (adsc.isPlaying)
            adsc.Stop();

        adsc.clip = rewards[Random.Range(0, rewards.Count)];
        adsc.Play();

        if (Random.Range(0, (int)Mathf.Floor(oneInEveryJumps)) != 0) return;

        reward.SetActive(false);
        reward.SetActive(true);
    }
}
