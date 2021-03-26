using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheaterItem : MonoBehaviour
{

    public float value;
    public AudioSource source;

    public void Awake() {
        //value = Random.Range(0, 1.0f);
    }
    public void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {            
            source.Play();
            //Send to controller
            if (value > 0)
                WeatherManager.Instance.ImproveWeather(gameObject);
            else
                WeatherManager.Instance.DeteriorateWeather(gameObject);
        }
    }
}
