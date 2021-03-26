using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monolit : MonoBehaviour
{
    public Animator animator;
    public Transform core;
    public MonolitListener listener;
    public float minSize;
    public float maxSize;
    public float minSpeed;
    public float maxSpeed;

    private void Awake() {
        var newScale = UnityEngine.Random.Range(minSize, maxSize);
        core.localScale = new Vector3(newScale, core.localScale.y, newScale);

        var speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        animator.speed = speed;

        listener.OnContactPlayer += OnContactPlayer;
    }

    private void OnContactPlayer() {
        WeatherManager.Instance.ApplyDamage();
    }

    private void OnDestroy() {
        if(listener != null)
            listener.OnContactPlayer -= OnContactPlayer;
    }
}
