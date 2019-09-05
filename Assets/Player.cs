using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Player : MonoBehaviour
{
    #region Attributes
    [Header("Refs")]
    public Rewarder rewarder;
    [Space(10)]

    [Header("Camera Shake")]
    [Range(0,10)]
    public float mag = 0;

    [Range(0, 5)]
    public float rough = 0;

    [Range(0, .25f)]
    public float fadeIn = 0;

    [Range(0, .25f)]
    public float fadeOut = 0;
    [Space(10)]

    [Header("Jump")]
    [Range(0, 10)]
    public float jumpForce = 0;

    [SerializeField]
    private bool jumpShakeToken = false;
    [SerializeField]
    private bool doubleJ = true;
    [Space(10)]

    [Header("SFX")]
    public AudioSource jumpSound;
    public AudioSource landSound;
    public AnimationCurve pitchCurve;
    #endregion

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private float jumpUseForce;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && doubleJ)
        {
            if (jumpShakeToken)
            {
                doubleJ = false;
                jumpUseForce = jumpForce * .9f;

                if (jumpSound.isPlaying)
                    jumpSound.Stop();
                jumpSound.pitch += .3f;
                jumpSound.Play();
            }
            else
            {
                PlaySound(jumpSound);
            }
            GetComponent<Rigidbody>().AddForce(transform.up * jumpUseForce, ForceMode.VelocityChange);
            StartCoroutine(waitAFrameSet());
        }
        if (GetComponent<Rigidbody>().velocity == Vector3.zero && jumpShakeToken)
        {
            jumpShakeToken = false;
            doubleJ = true;
            PlaySound(landSound);
            jumpUseForce = jumpForce * 1.1f;
            CameraShaker.Instance.ShakeOnce(mag, rough, fadeIn, fadeOut);
            rewarder.Reward();
        }
    }

    IEnumerator waitAFrameSet()
    {
        yield return null;
        jumpShakeToken = true;
    }


    void PlaySound(AudioSource source)
    {
        if (source.isPlaying)
            source.Stop();
        source.pitch = pitchCurve.Evaluate(Random.value);
        source.Play();
    }
}
