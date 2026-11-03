using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Audio Clips")]
    public AudioClip crystalCollectSound;

    [Header("Audio Filter")]
    private AudioLowPassFilter bgmLowPassFilter;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (bgmAudioSource != null)
        {
            bgmLowPassFilter = bgmAudioSource.gameObject.GetComponent<AudioLowPassFilter>();
            if (bgmLowPassFilter == null)
            {
                bgmLowPassFilter = bgmAudioSource.gameObject.AddComponent<AudioLowPassFilter>();
            }
            bgmLowPassFilter.enabled = false;
        }
    }

    public void PlaySFXPitched(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f); 
            #if UNITY_WEBGL && !UNITY_EDITOR
                sfxAudioSource.pitch = 1.0f;
            #else
                sfxAudioSource.pitch = Random.Range(0.95f, 1.05f);
            #endif

            sfxAudioSource.PlayOneShot(clip, sfxVol);
        }
    }
   

    public void TriggerGameOverFilter(bool enable)
    {
        if (bgmLowPassFilter != null)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothFilterRoutine(enable));
        }
    }

    System.Collections.IEnumerator SmoothFilterRoutine(bool enable)
    {
        bgmLowPassFilter.enabled = true;
        float startFreq = bgmLowPassFilter.cutoffFrequency;
        float targetFreq = enable ? 600f : 22000f;
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            bgmLowPassFilter.cutoffFrequency = Mathf.Lerp(startFreq, targetFreq, t);
            yield return null;
        }

        bgmLowPassFilter.cutoffFrequency = targetFreq;
        if (!enable) bgmLowPassFilter.enabled = false;
    }
}