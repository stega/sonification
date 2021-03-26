using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsSpawmer : MonoBehaviour
{
    public GameObject goodItem;
    public GameObject badItem;
    public Transform centerPlanet;
    public float radius = 5;   
    public int minNumberOfItems = 10;
    public int maxNumberOfItems = 30;
    public bool spawnOnStart = false;

    public int NumberOfItems { get; private set; }
    public int TotalBadItems { get; private set; }
    public int TotalGoodItems { get; private set; }
    public int CurrentBadItems => badItems_.Count;
    public int CurrentGoodItems => goodItems_.Count;


    private void Start() {
        if (spawnOnStart) {
            Spawn();
        }
    }

    public void Spawn() {
        NumberOfItems = Random.Range(minNumberOfItems, maxNumberOfItems);
        Spawn(NumberOfItems);
    }

    public void Spawn(int itemsAmount) {
        for (int i = 0; i < itemsAmount; i++) {
            GameObject baseItem;
            var isGoodItem = i % 2 == 0;
            if (isGoodItem) {
                baseItem = goodItem;
                TotalGoodItems++;
            } else {
                baseItem = badItem;
                TotalBadItems++;
            }
            var newItem = Instantiate<GameObject>(baseItem);
            newItem.transform.parent = centerPlanet.transform;
            newItem.transform.position = centerPlanet.transform.position + Random.onUnitSphere * radius;
            newItem.transform.up = (newItem.transform.position - centerPlanet.transform.position).normalized;
            if (isGoodItem)
                goodItems_.Add(newItem);
            else
                badItems_.Add(newItem);
        }
    }

    public void DestroyItem(GameObject item) {
        if (goodItems_.Contains(item)) {
            goodItems_.Remove(item);
            Destroy(item);
        }

        if (badItems_.Contains(item)) {
            badItems_.Remove(item);
            Destroy(item);
        }
    }

    public void DestroyAndResetAllItems() {
        for (int i = 0; i < goodItems_.Count; i++) {
            Destroy(goodItems_[i]);
        }
        for (int i = 0; i < badItems_.Count; i++) {
            Destroy(badItems_[i]);
        }
        goodItems_.Clear();
        badItems_.Clear();
        TotalBadItems = 0;
        TotalGoodItems = 0;
    }

    List<GameObject> goodItems_ = new List<GameObject>();
    List<GameObject> badItems_ = new List<GameObject>();
}
