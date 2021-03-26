using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonolitListener : MonoBehaviour
{
    public Action OnContactPlayer;
    public void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (OnContactPlayer != null)
                OnContactPlayer();
        }
    }
}
