using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightFlicker : MonoBehaviour
{
    [Tooltip("External light to flicker; you can leave this null if you attach script to a light")]
    public Light lightObj;
    [Tooltip("Minimum random light intensity")]
    public float minIntensity = 0f;
    [Tooltip("Maximum random light intensity")]
    public float maxIntensity = 1f;
    [Tooltip("How much to smooth out the randomness; lower values = sparks, higher = lantern")]
    [Range(1, 50)]
    public int smoothing = 5;

    Queue<float> smoothQueue = new Queue<float>();
    float lastSum = 0;

    public void Reset()
    {
        smoothQueue.Clear();
        lastSum = 0;
    }

    void OnEnable()
    {
        Reset();

        smoothQueue = new Queue<float>(smoothing);
        // External or internal light?
        if (lightObj == null)
        {
            lightObj = GetComponent<Light>();
        }
    }

    void Update()
    {
        if (lightObj == null)
            return;

        // pop off an item if too big
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        // Generate random new item, calculate new average
        float newVal = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newVal);
        lastSum += newVal;

        // Calculate new smoothed average
        GetComponent<Light>().intensity = lastSum / (float)smoothQueue.Count;
    }
}
