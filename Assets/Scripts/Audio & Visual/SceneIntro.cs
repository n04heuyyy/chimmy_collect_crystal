using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneIntro : MonoBehaviour
{
    private Image fadeImage; // Menggunakan image kosong hitam yang diubah transparansinya

    void Awake()
    {
        fadeImage = GetComponent<Image>();
        // Mulai dengan layar hitam total (alpha 1)
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.raycastTarget = true; // Blokir klik
    }

    void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        float duration = 1.0f;
        float elapsed = 0;

        while (elapsed < duration) 
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / duration); // Buat perlahan transparan
            fadeImage.color = new Color(0, 0, 0, alpha); 
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0); // Pastikan sepenuhnya transparan
        fadeImage.raycastTarget = false; // Buka blokir klik
    }
}