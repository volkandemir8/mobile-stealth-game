using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleCount : MonoBehaviour
{
    TMPro.TMP_Text text;
    public static int count;

    private void Awake()
    {
        count = 0;
        text = GetComponent<TMPro.TMP_Text>();
    }

    private void Start()
    {
        UpdateCount();
    }

    void OnEnable() => Collectibles.OnCollected += OnCollectibleCollected;
    void OnDisable() => Collectibles.OnCollected -= OnCollectibleCollected;


    void OnCollectibleCollected()
    {
        count++;
        UpdateCount();
    }

    void UpdateCount()
    {
        text.text = $"{count}/{Collectibles.total}";
    }
}
