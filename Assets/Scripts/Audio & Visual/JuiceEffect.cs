using UnityEngine;
using System.Collections;

public static class JuiceEffects
{
    // Hit Stop effect
    public static IEnumerator DoHitStop(float duration)
    {
        Time.timeScale = 0.0f; // Time Freeze
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f; // Kembali normal
    }
}