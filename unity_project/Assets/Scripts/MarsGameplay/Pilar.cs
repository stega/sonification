using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pilar : MonoBehaviour
{
    public Transform head;
    public Transform body;
    public Renderer render;
    public MonolitListener listener;
    public float minHeight;
    public float maxHeight;
    public float growingSpeed;
    public Gradient weatherGradient;

    bool growing_ = true;

    private void Awake() {
        listener.OnContactPlayer += OnContactPlayer;
    }

    float deltaSpeed_ = 0;
    private void Start() {
        var speedMode = WeatherManager2.Instance.GetPilarSpeedMode();
        if (speedMode == 0)
            deltaSpeed_ = -growingSpeed * 0.5f;
        if (speedMode == 2)
            deltaSpeed_ = growingSpeed * 2f;

        SetHight(minHeight);
    }

    private void Update() {
        if (growing_) {
            SetHight(body.localScale.y + (growingSpeed + deltaSpeed_) * Time.deltaTime);
            SetColor();
            if (body.localScale.y >= maxHeight) {
                var pos = head.localPosition;
                pos.y = body.localScale.y;
                head.localPosition = pos;
                head.gameObject.SetActive(true);
                growing_ = false;
            }
        }
    }

    void SetHight(float hight) {
        var scale = body.localScale;
        scale.y = hight;
        body.localScale = scale;
    }

    void SetColor() {
        render.material.color = weatherGradient.Evaluate(body.localScale.y / maxHeight);
    }

    private void OnContactPlayer() {
        WeatherManager2.Instance.HitPilar(Mathf.InverseLerp(minHeight, maxHeight, body.localScale.y));
        Destroy(gameObject);
    }

    private void OnDestroy() {
        if (listener != null)
            listener.OnContactPlayer -= OnContactPlayer;
    }
}
