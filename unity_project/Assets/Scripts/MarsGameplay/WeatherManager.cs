using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{
    public enum Mode { Improve, Degrade}

    public OSC osc;

    [Header("Mode Balance")]
    public float modeChangePeriod = 10;

    //[Header("UI")]
    [Header("Spawners")]
    public ItemsSpawmer itemsSpawner;
    public ItemsSpawmer pilarsSpawner;
    public ItemsSpawmer aliensSpawner;

    [Header("UI")]
    public Text scoreLabel;
    public Text lifeLabel;
    public Text modeLabel;
    public Text timerLabel;
    public Color colorOnImprove;
    public Color colorOnDegrade;

    public static WeatherManager Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        mode_ = Mode.Improve;
        ChangeDistribution();

        //OSC receivers
        osc.SetAddressHandler("/min_temp", OnReceiveMinTemp);
    }

    #region OSC Management

    private void OnReceiveMinTemp(OscMessage oscM) {
        var minTemp = oscM.values[0] as string;
        Debug.LogWarning(minTemp);
    }

    OscMessage oscMessage_ = new OscMessage();
    void SenOSC(string address, int value) {
        //oscMessage_ = new OscMessage();
        oscMessage_.address = address;
        oscMessage_.values.Clear();
        oscMessage_.values.Add(value);
        osc.Send(oscMessage_);
        Debug.Log(value);
    }

    #endregion

    private void Update() {
        timer_ += Time.deltaTime;
        if (timer_ >= modeChangePeriod || FinishConditionMet()) {
            switch (mode_) {
                case Mode.Improve:
                    mode_ = Mode.Degrade;
                    break;
                case Mode.Degrade:
                    mode_ = Mode.Improve;
                    break;
                default:
                    break;
            }
            timer_ = 0;
            ResetCounters();
            ChangeDistribution();
        }
        UpdateTimer();
    }

    #region Public Interface

    public void ApplyDamage() {
        damageCount_++;
        UpdateDamage();
    }

    public void ImproveWeather(GameObject item) {
        goodItemsCount_++;
        ChangeScore(true, item);
    }

    public void DeteriorateWeather(GameObject item) {
        badItemsCount_++;
        ChangeScore(false, item);
    }

    #endregion

    #region Operations

    void ChangeScore(bool improve, GameObject item) {
        itemsSpawner.DestroyItem(item);
        switch (mode_) {
            case Mode.Improve:
                score_ += improve ? 1 : -1;
                break;
            case Mode.Degrade:
                score_ += !improve ? 1 : -1;
                break;
            default:
                break;
        }
        score_ = score_ < 0 ? 0 : score_;
        UpdateScore();
    }


    void ResetCounters() {
        goodItemsCount_ = 0;
        badItemsCount_ = 0;
    }

    void ChangeDistribution() {
        itemsSpawner.DestroyAndResetAllItems();
        pilarsSpawner.DestroyAndResetAllItems();
        aliensSpawner.DestroyAndResetAllItems();

        itemsSpawner.Spawn();
        pilarsSpawner.Spawn();
        aliensSpawner.Spawn();

        UpdateMode();
    }

    bool FinishConditionMet() {
        switch (mode_) {
            case Mode.Improve:
                return goodItemsCount_ >= itemsSpawner.TotalGoodItems;
            case Mode.Degrade:
                return badItemsCount_ >= itemsSpawner.TotalBadItems;
            default:
                break;
        }
        return false;
    }

    #endregion

    #region Feedback

    void UpdateScore() { //Feedback update
        scoreLabel.text = score_ + "";
    }

    void UpdateDamage() {
        var percentaje = 100 - damageCount_;
        lifeLabel.text = percentaje + "%";
        SenOSC("/temp", percentaje);
    }

    void UpdateMode() {
        modeLabel.text = mode_.ToString();
        modeLabel.color = mode_ == Mode.Improve ? colorOnImprove : colorOnDegrade;
    }

    void UpdateTimer() {
        var remaingSeconds = (int)(modeChangePeriod - timer_);
        timerLabel.text = string.Format("00:{0:D2}", remaingSeconds);
    }

    #endregion

    int damageCount_;
    int badItemsCount_;
    int goodItemsCount_;

    int score_;

    Mode mode_;

    float timer_;
}
