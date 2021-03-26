using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WeatherManager2 : MonoBehaviour
{
    public enum State { Playing, Death}

    [System.Serializable]
    public struct AudioVariations {
        public AudioClip[] varitions;
    }

    public OSC osc;

    [Header("OSC Endpoints")]
    public string pilarEP = "/pillar_norm";
    public string gameStateEP = "/deaded";

    [Header("Controllers")]
    public GameObject player;
    public ItemsSpawmer pilarsSpawner;
    public ItemsSpawmer aidKiSpawner;
    public PostProcessingBehaviour postProcessingBehaviour;

    [Header("Balance")]
    public int periodForNewPilars = 2;
    public int periodForNewAidKits = 30;
    public float damageFactor = 2;

    [Header("UI")]
    public Text scoreLabel;
    public Text lifeLabel;
    public Transform lifeBar;
    public Text stateLabel;    
    public GameObject gameOverPanel;

    [Header("Visual Mapping")]
    public Renderer planetRender;
    public Gradient planetColor;
    public ParticleSystem particlesSpace;
    public Light[] environmentalLights;
    public float minLightValue;
    public float maxLightValue;


    [Header("Audio")]
    public AudioSource pilarsHitSource;
    public AudioVariations[] pilarsHitClips;
    public AudioSource newGameSound;
    public AudioSource deathGameSound;



    float timerForNewPilars_;
    float timerForNewAidKits_;
    int score_;
    int damageCount_;
    State state_;

    public static WeatherManager2 Instance;

    private void Awake() {
        Instance = this;
        //Send Message when game start
        state_ = State.Playing;
        newGameSound.Play();
        SenOSC(gameStateEP, 1);
        
    }

    private void Start() {
        //OSC receivers
        osc.SetAddressHandler("/weather_norm", OnReceiveWeather);
        osc.SetAddressHandler("/pressure_norm", OnReceivePressure);
        osc.SetAddressHandler("/radiation_norm", OnReceiveRadiation);        
    }

    int weatherFactor_ = 2;
    private void OnReceiveWeather(OscMessage oscM) {
        var weather = (int)oscM.values[0];
        weatherFactor_ = weather;
        var color = planetColor.Evaluate(weather == 1 ? 0 : (weather == 2 ? 0.5f : 1f));
        planetRender.material.color = color;
        particlesSpace.startColor = color;
        RenderSettings.skybox.SetColor("_Tint", color);
        switch (weather) {
            case 1:
                stateLabel.text = "COLD";                
                break;
            case 2:                
                stateLabel.text = "MODERATE";
                break;
            case 3:                
                stateLabel.text = "HOT";
                break;
            default:
                break;
        }
        stateLabel.color = color;
    }

    float currentPressure_ = 0.5f;
    private void OnReceivePressure(OscMessage oscM) {
        float pressure = 0;
        if (oscM.values[0] is float pf)
            pressure = pf;
        if (oscM.values[0] is int pi)
            pressure = pi;
        currentPressure_ = pressure;
        var mBlurSett = postProcessingBehaviour.profile.motionBlur.settings;
        mBlurSett.frameBlending = pressure;
        postProcessingBehaviour.profile.motionBlur.settings = mBlurSett;
    }

    private void OnReceiveRadiation(OscMessage oscM) {
        var radiation = (int)oscM.values[0];
        var factor = Mathf.InverseLerp(1, 4, radiation);
        var ligthIntensity = Mathf.Lerp(minLightValue, maxLightValue, factor);
        for (int i = 0; i < environmentalLights.Length; i++) {
            environmentalLights[i].intensity = ligthIntensity;
        }
    }

    void Update() {
        switch (state_) {
            case State.Playing:
                timerForNewPilars_ += Time.deltaTime;
                if (timerForNewPilars_ >= periodForNewPilars) {
                    pilarsSpawner.Spawn(1);
                    timerForNewPilars_ = 0;
                }

                timerForNewAidKits_ += Time.deltaTime;
                if (timerForNewAidKits_ >= periodForNewAidKits) {
                    aidKiSpawner.Spawn(1);
                    timerForNewAidKits_ = 0;
                }
                break;
            case State.Death:
                if(Input.GetKeyDown(KeyCode.Return)){
                    Restart();
                }
                break;
            default:
                break;
        }
        
    }

    OscMessage oscMessage_ = new OscMessage();
    void SenOSC(string address, object value) {
        //oscMessage_ = new OscMessage();
        oscMessage_.address = address;
        oscMessage_.values.Clear();
        oscMessage_.values.Add(value);
        osc.Send(oscMessage_);
    }

    #region Gameplay Actions

    public void HitPilar(float pilarValue/*, bool spwanNewOnes*/) {
        //if(spwanNewOnes)
        //    pilarsSpawner.Spawn(newPilarsWhenHit);
        UpdateScore((int)(pilarValue * pilarValue * 100 * 100));//(100*val)^2
        UpdateDamage((int)(pilarValue * 100 / damageFactor));

        PlayPilarSound(pilarValue);

        SenOSC(pilarEP, pilarValue);
    }

    public void AddHealth(int amount) {
        UpdateDamage(-amount);
    }

    public int GetPilarSpeedMode() {
        if (currentPressure_ < 0.33f)
            return 0;
        if (currentPressure_ > 0.66f)
            return 2;
        else
            return 1;
        
    }

    #endregion

    #region Operations

    void UpdateScore(int additionalScore) { //Feedback update
        score_ += additionalScore;
        scoreLabel.text = score_ + "";
    }

    void UpdateDamage(int damage) {
        damageCount_ = Mathf.Clamp(damageCount_ + damage, 0, 100); 
        var percentaje = 100 - damageCount_;
        lifeLabel.text = percentaje + "%";
        lifeBar.localScale = new Vector3(percentaje / 100f, 1, 1);
        if (percentaje <= 0)
            Death();
    }

    void Death() {
        gameOverPanel.SetActive(true);
        Destroy(player);
        state_ = State.Death;
        deathGameSound.Play();
        SenOSC(gameStateEP, 0);
    }

    void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Audio

    void PlayPilarSound(float pilarValue) {
        var index = Mathf.Clamp((int)(pilarsHitClips.Length * pilarValue), 0, pilarsHitClips.Length - 1);
        pilarsHitSource.PlayOneShot(pilarsHitClips[index].varitions[Random.Range(0, pilarsHitClips[index].varitions.Length)]);
    }

    #endregion
}
