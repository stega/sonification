using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    public AudioSource engineSound;
    public float pitchOnFast;
    public float pitchOnSlow;
    public float panOnRight;
    public float panOnLeft;

    float lastPitch_;
    float lastPan_;

    // Update is called once per frame
    void Update() {
        var forwardAcceleration = Input.GetAxis("Vertical");
        lastPitch_ = 1f;
        if (forwardAcceleration > 0.5)
            lastPitch_ = pitchOnFast;
        else if (forwardAcceleration < -0.5)
            lastPitch_ = pitchOnSlow;
        float vel = 0;
        engineSound.pitch = Mathf.SmoothDamp(engineSound.pitch, lastPitch_, ref vel, 0.15f);

        var direction = Input.GetAxisRaw("Horizontal");
        lastPan_ = 0;
        if (direction > 0.5)
            lastPan_ = panOnRight;
        else if (direction < -0.5)
            lastPan_ = panOnLeft;
        engineSound.panStereo = Mathf.SmoothDamp(engineSound.panStereo, lastPan_, ref vel, 0.15f);
    }
}
