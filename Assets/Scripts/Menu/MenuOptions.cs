using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MenuOptions : MonoBehaviour
{
    [Header("Audio Controllers")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public AudioSource mainMenuBGM;

    [Header("SFX Preview")]
    public AudioSource previewAudioSource;
    public AudioClip previewSFXClip;

    [Header("Graphics & Performance")]
    public TMP_Dropdown fpsDropdown;

    void Start()
    {
        // Ambil data volume musik yang tersimpan
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = savedMusic;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume); // Simpan data real-time
        }
        
        // Begitu main menu dibuka, langsung apply volume
        if (mainMenuBGM != null) mainMenuBGM.volume = savedMusic;

        // Ambil data volume SFX yang tersimpan
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = savedSFX;
            sfxVolumeSlider.onValueChanged.AddListener(SaveSFXVolume);
        }

        // Setup Dropdown FPS
        if (fpsDropdown != null)
        {
            fpsDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string> { "30 FPS", "60 FPS", "Unlimited" };
            fpsDropdown.AddOptions(options);
            fpsDropdown.value = PlayerPrefs.GetInt("FPSSetting", 1);
            fpsDropdown.onValueChanged.AddListener(SetFPSLimit);
            ApplyFPSLimit(fpsDropdown.value);
        }
    }

    public void SetMusicVolume(float value) // Simpan volume musik
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
        // Langsung ubah volume musik menu utama secara realtime
        if (mainMenuBGM != null) mainMenuBGM.volume = value;
    }

    public void SaveSFXVolume(float value) // Simpan volume SFX
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    // Putar preview sfx ketika melepas pointer (menggunakan event trigger pointer up)
    public void PlaySFXPreviewOnPointerUp()
    {
        if (previewAudioSource != null && previewSFXClip != null)
        {
            // Ambil volume yang disimpan saat digeser
            float currentVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            
            // Setel volume audio source
            previewAudioSource.volume = currentVolume;

            // Putar suara sekali
            previewAudioSource.PlayOneShot(previewSFXClip, currentVolume);
        }
    }

    public void SetFPSLimit(int index)
    {
        PlayerPrefs.SetInt("FPSSetting", index);
        PlayerPrefs.Save();
        ApplyFPSLimit(index);
    }

    void ApplyFPSLimit(int index)
    {
        if (index == 0) Application.targetFrameRate = 30;
        else if (index == 1) Application.targetFrameRate = 60;
        else Application.targetFrameRate = -1; // Tanpa batasan (Unlimited)
    }
}