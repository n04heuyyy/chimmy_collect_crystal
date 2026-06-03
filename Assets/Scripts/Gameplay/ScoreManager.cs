using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; } // Instansi agar dapat digunakan skrip lain
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;

    void Awake()
    {
        // Set referensi ke dirinya sendiri
        if (instance == null) instance = this;
        else Destroy(gameObject); // Hancurkan duplikat jika ada
    }

    // Ambil skor (digunakan GameManager)
    public int GetScore()
    {
        return currentScore;
    }

    public void AddScore(int points)
    {
        currentScore += points;
        scoreText.text = "Score: " + currentScore;

        // Pastikan kita mengambil parent (kotak skor) untuk dianiamsikan
        Transform target = scoreText.transform.parent;
        // Jalankan animasi
        StartCoroutine(PopAnimation(scoreText.transform.parent, 1.2f));
    }

    public IEnumerator PopAnimation(Transform target, float scaleAmount) // Animasi pop kotak skor
    {
        if (target == null) yield break;
        float duration = 0.2f; 
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float curve = Mathf.Sin((elapsed / duration) * Mathf.PI);
            float s = 1f + (curve * (scaleAmount - 1f));
            
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        
        // Paksa kembali ke posisi awal
        target.localScale = Vector3.one;
    }
}