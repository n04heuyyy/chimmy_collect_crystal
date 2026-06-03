using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")]
    public RectTransform gameOverPanel;
    public RectTransform nameInputPanel;
    public RectTransform actionButtonsPanel;

    [Header("Gameplay UI to Eject")]
    public RectTransform gameplayScoreGroup;
    public RectTransform gameplayHPGroup;

    [Header("UI Elements")]
    public TMP_Text finalScoreText;
    public TMP_InputField nameInputField;
    public Image fadeImage;
    public TMP_Text crystalsCollectedText;
    public TMP_Text elapsedTimeText;
    private int achievedScore;
    private float finalSurvivalDuration;

    [Header("Animation Settings")]
    public float animDuration = 1.0f;

    public AudioClip deadSound;
    
    private string currentModeKey;
    private bool isGameOverTriggered = false; // Pengaman agar tidak terpanggil berkali-kali

    // Simpan waktu mulai gameplay agar dapat menghitung waktu di game
    private float gameStartTime;

    void Awake()
    {
        // Posisi panel disembunyikan di luar layar
        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(false);
            gameOverPanel.anchoredPosition = new Vector2(0f, -3000f); // Taruh jauh di bawah layar
        }
        if (nameInputPanel != null)
        {
            nameInputPanel.anchoredPosition = new Vector2(0f, 3000f); // Taruh jauh di atas layar
        }
        if (actionButtonsPanel != null)
        {
            actionButtonsPanel.anchoredPosition = new Vector2(0f, 0f); 
        }
        // Pastikan layar fade transparan
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
        }
    }

    void Start()
    {
        // Untuk sinkronisasi mode, buat ke huruf kecil agar sesuai
        string mode = PlayerPrefs.GetString("GameMode", "classic").ToLower();
        bool isOneHit = PlayerPrefs.GetInt("OneHitKO", 0) == 1;
        
        // Hasil format kunci seragam: "classic", "classic_ohko", "unique", "unique_ohko"
        currentModeKey = mode + (isOneHit ? "_ohko" : "");
    }
    public void StartSurvivalTimer() // Hitung waktu di game
    {
        gameStartTime = Time.time;
    }

    public void TriggerGameOver(int finalScore, int totalCrystals)
    {
        // Hentikan eksekusi jika game over sudah pernah dipicu sebelumnya
        if (isGameOverTriggered) return;
        isGameOverTriggered = true;

        float currentSFXVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        GameManager.instance.sfxAudioSource.PlayOneShot(deadSound, currentSFXVol);

        achievedScore = finalScore;
        finalScoreText.text = "FINAL SCORE: " + achievedScore; // Tampilkan final score

        // Hitung durasi murni (Waktu saat ini dikurangi waktu mulai setelah intro)
        finalSurvivalDuration = Time.time - gameStartTime; 
        if (finalSurvivalDuration < 0) finalSurvivalDuration = 0;

        int minutes = Mathf.FloorToInt(finalSurvivalDuration / 60f);
        int seconds = Mathf.FloorToInt(finalSurvivalDuration % 60f);
        string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds); // Format MM:SS

        if (crystalsCollectedText != null) // Tampilkan jumlah kristal diambil
        {
            crystalsCollectedText.text = $"Crystals Collected: {totalCrystals}";
        }
        if (elapsedTimeText != null) // Tampilkan durasi waktu game
        {
            elapsedTimeText.text = $"Time Survived: {timeFormatted}";
        }

        // Matikan kontrol pergerakan player di GameManager secara instan
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            GameManager.instance.player.canMove = false;
        }

        StartCoroutine(GameOverSequenceRoutine());
    }

    IEnumerator GameOverSequenceRoutine()
    {
        // Hentikan waktu gameplay agar musuh/kristal berhenti berinteraksi di background
        Time.timeScale = 0f; 

        // Keluarkan UI gameplay (kotak HP, skor)
        float elapsedOut = 0f;
        float durationOut = 0.4f; 
        
        Vector2 scoreStart = gameplayScoreGroup.anchoredPosition;
        Vector2 hpStart = gameplayHPGroup.anchoredPosition;

        // Letakkan di luar layar
        Vector2 scoreTarget = scoreStart + new Vector2(0f, 1200f);
        Vector2 hpTarget = hpStart + new Vector2(0f, 1200f);

        // Karena Time.timeScale = 0, pakai Time.unscaledDeltaTime agar coroutine tetap berjalan
        while (elapsedOut < durationOut)
        {
            elapsedOut += Time.unscaledDeltaTime; 
            float t = Mathf.SmoothStep(0f, 1f, elapsedOut / durationOut);
            
            gameplayScoreGroup.anchoredPosition = Vector2.Lerp(scoreStart, scoreTarget, t);
            gameplayHPGroup.anchoredPosition = Vector2.Lerp(hpStart, hpTarget, t);
            yield return null;
        }

        gameplayScoreGroup.gameObject.SetActive(false); // Matikan agar efisiensi
        gameplayHPGroup.gameObject.SetActive(false);

        // Cek apakah skor masuk leaderboard        
        bool fitsLeaderboard = CheckIfFitsInLeaderboard(currentModeKey, achievedScore);
        if (fitsLeaderboard)
        {
            // Posisikan subpanel new high score ke atas
            nameInputPanel.anchoredPosition = new Vector2(0f, 3000f); 
            nameInputPanel.gameObject.SetActive(true);
            nameInputField.text = "";
        }
        else
        {
            // Nonaktifkan panel new high score, aktifkan pa
            nameInputPanel.gameObject.SetActive(false);
            actionButtonsPanel.anchoredPosition = new Vector2(0f, 0f); 
        }

        // Aktifkan panel game over dan action button
        actionButtonsPanel.gameObject.SetActive(true); 
        gameOverPanel.gameObject.SetActive(true);

        // Geser Ppanel game over ke layar
        float elapsedIn = 0f;
        while (elapsedIn < animDuration)
        {
            elapsedIn += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedIn / animDuration);
            float currentY = Mathf.Lerp(-1400f, 0f, t);
            
            gameOverPanel.anchoredPosition = new Vector2(0f, currentY);
            yield return null;
        }
        gameOverPanel.anchoredPosition = new Vector2(0f, 0f);


        // Jika high score, geser panel new high score
        if (fitsLeaderboard)
        {
            float elapsedHighScore = 0f;
            float durationHighScore = 0.6f; 

            while (elapsedHighScore < durationHighScore)
            {
                elapsedHighScore += Time.unscaledDeltaTime; // Gunakan unscaledDeltaTime
                float t = Mathf.SmoothStep(0f, 1f, elapsedHighScore / durationHighScore);
                
                float currentY = Mathf.Lerp(3000f, 0f, t);
                nameInputPanel.anchoredPosition = new Vector2(0f, currentY);
                yield return null;
            }
            nameInputPanel.anchoredPosition = new Vector2(0f, 0f);
            nameInputField.Select(); 
        }
    }

    bool CheckIfFitsInLeaderboard(string modeKey, int score)
    {
        // Skor 0 ke bawah tidak boleh menampilkan new high score
        if (score <= 0) return false;

        int tenthPlaceScore = PlayerPrefs.GetInt($"{modeKey}_Score_9", 0); // Skor paling bawah di leaderboard
        int currentCount = PlayerPrefs.GetInt($"{modeKey}_Score_Count", 0); // Jumlah data di leaderboard

        // Jika slot 10 besar belum penuh, skor > 0 wajib menampilkan new high score
        if (currentCount < 10) return true;
        // Jika slot sudah penuh 10 data, baru bandingkan dengan skor peringkat ke-10
        return score > tenthPlaceScore;
    }

    public void SubmitNameAndSave()
    {
        string playerName = nameInputField.text.Trim();
        
        // Kalau nama kosong atau spasi murni, buat anonim
        if (string.IsNullOrEmpty(playerName)) playerName = "Anonymous";
        // Batasi panjang karakter agar tidak merusak layout papan peringkat
        if (playerName.Length > 12) playerName = playerName.Substring(0, 12);

        // Masukkan nama, skor, durasi game ke leaderboard sesuai mode key
        InsertNewHighScore(currentModeKey, playerName, achievedScore, Mathf.FloorToInt(finalSurvivalDuration));
        StartCoroutine(SubmitAnimationRoutine()); 
    }

    IEnumerator SubmitAnimationRoutine()
    {
        float elapsedSubmit = 0f;
        float durationSubmit = 0.6f;

        Vector2 nameStartPos = nameInputPanel.anchoredPosition;
        Vector2 nameTargetPos = new Vector2(0f, 3000f);

        while (elapsedSubmit < durationSubmit)
        {
            elapsedSubmit += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedSubmit / durationSubmit);

            // Angkat panel dan isinya ke luar layar
            nameInputPanel.anchoredPosition = Vector2.Lerp(nameStartPos, nameTargetPos, t);

            yield return null;
        }

        nameInputPanel.anchoredPosition = nameTargetPos;
        nameInputPanel.gameObject.SetActive(false);
    }

    void InsertNewHighScore(string modeKey, string name, int score, int timeInSeconds)
    {
        string[] names = new string[11];
        int[] scores = new int[11];
        int[] times = new int[11];
        int currentCount = PlayerPrefs.GetInt($"{modeKey}_Score_Count", 0);

        // Ambil data 10 besar lama ke memori lokal array
        for (int i = 0; i < 10; i++)
        {
            names[i] = PlayerPrefs.GetString($"{modeKey}_Name_{i}", "---");
            scores[i] = PlayerPrefs.GetInt($"{modeKey}_Score_{i}", 0);
            times[i] = PlayerPrefs.GetInt($"{modeKey}_Time_{i}", 0);
        }

        // Masukkan skor baru ke slot cadangan
        names[10] = name;
        scores[10] = score;
        times[10] = timeInSeconds;

        // 3. Bubble Sort data berdasarkan skor tertinggi, lalu waktu terlama
        for (int i = 0; i < 11; i++)
        {
            for (int j = i + 1; j < 11; j++)
            {
                bool swapNeeded = false;

                if (scores[j] > scores[i])
                {
                    swapNeeded = true;
                }
                // Jika skor persis sama, cek siapa yang durasi waktunya lebih besar (lama)
                else if (scores[j] == scores[i])
                {
                    if (times[j] > times[i])
                    {
                        swapNeeded = true;
                    }
                }

                if (swapNeeded)
                {
                    // Tukar Skor, nama, dan waktu
                    int tempScore = scores[i];
                    scores[i] = scores[j];
                    scores[j] = tempScore;

                    string tempName = names[i];
                    names[i] = names[j];
                    names[j] = tempName;

                    int tempTime = times[i];
                    times[i] = times[j];
                    times[j] = tempTime;
                }
            }
        }

        // Tulis kembali hanya 10 data teratas ke PlayerPrefs perangkat
        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetString($"{modeKey}_Name_{i}", names[i]);
            PlayerPrefs.SetInt($"{modeKey}_Score_{i}", scores[i]);
            PlayerPrefs.SetInt($"{modeKey}_Time_{i}", times[i]);
        }

        // Tambah total data jika slot belum penuh 10
        if (currentCount < 10)
        {
            PlayerPrefs.SetInt($"{modeKey}_Score_Count", currentCount + 1);
        }

        PlayerPrefs.Save();
    }
    
    public void RestartGame() // Jalankan lagi scene gameplay
    {
        EnemyController.speedBonus = 0f;
        StartCoroutine(FadeAndLoadScene(SceneManager.GetActiveScene().name));
    }

    public void LoadMainMenu() // Kembali ke scene menu
    {
        StartCoroutine(FadeAndLoadScene("CCC Menu")); 
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true; // Blokir klik
            
            float elapsed = 0f;
            float fadeDuration = 0.8f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Wajib unscaledDeltaTime karena timescale sedang 0
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }
        }

        // Reset skala waktu kembali normal sebelum berganti scene agar scene tujuan tidak ikut freeze!
        Time.timeScale = 1f; 
        SceneManager.LoadScene(sceneName);
    }
}