using UnityEngine;

public class UIPulse : MonoBehaviour
{
    public float pulseSpeed = 5f;
    public float pulseAmount = 1.15f;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Membuat skala berdetak naik-turun menggunakan fungsi Sinusoidal
        float scaleMultiplier = Mathf.Lerp(1f, pulseAmount, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        transform.localScale = baseScale * scaleMultiplier;
    }
}