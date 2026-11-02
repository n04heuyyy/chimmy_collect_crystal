using UnityEngine;
using System.Collections;

public class WorldShake : MonoBehaviour
{
    public static WorldShake instance;
    private Vector3 originalLocalPos;

    void Awake()
    {
        instance = this;
        originalLocalPos = transform.localPosition;
    }

    // Trigger world shake
    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Mix localpos 
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to normal
        transform.localPosition = originalLocalPos;
    }
}