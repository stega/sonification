using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AidKit : MonoBehaviour
{
    public MonolitListener listener;
    public int healthAmount = 20;

    [Header("Audio")]
    public AudioSource source;
    public float volumeVariation;
    public float pitchVariation;

    float initVolume_;

    private void Awake() {
        listener.OnContactPlayer += OnContactPlayer;
        initVolume_ = source.volume;
    }

    private void OnContactPlayer() {
        WeatherManager2.Instance.AddHealth(healthAmount);
        PlaySound();
        Destroy(gameObject);
    }

    void PlaySound() {        
        source.volume = initVolume_ + Random.Range(-volumeVariation, volumeVariation);
        source.pitch = 1 + Random.Range(-pitchVariation, pitchVariation);
        source.Play();
        source.transform.parent = null;
        Destroy(source.gameObject, source.clip.length);
    }

    private void OnDestroy() {
        if (listener != null)
            listener.OnContactPlayer -= OnContactPlayer;
    }

}
