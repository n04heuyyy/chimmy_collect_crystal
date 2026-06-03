using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    private AudioSource bgm;

    void Start()
    {
        // Menggunakan Audi Source dengan output musik bgm
        bgm = GetComponent<AudioSource>();
        StartCoroutine(PlayMusicWithDelay());
    }


    IEnumerator PlayMusicWithDelay()
    {
        // Ada 5 detik intro sebelum musik
        yield return new WaitForSeconds(5f);

        // Ambil maks slider volume musik di panel opsi
        float targetVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        // Mulai putar musik, gunakan volume yang diambil
        bgm.Play();
        bgm.volume = targetVolume;
    }
}