using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlayerController player;
    public GameOverManager gameOverManager;

    [Header("Audio & UI")]
    public AudioSource sfxAudioSource;  
    public AudioSource bgmAudioSource;
    public AudioClip startSound1; // Ini buat suara intro 4 detik
    public AudioClip startSound2; // Lalu 1 detik audio EUYYY
    public TMP_Text introCountdownText;
    public AudioSource bgmNormal; // Ada 2 buah bgm untuk low pass filter effect
    public AudioSource bgmMuffled; // Yang ini low pass'd dan ditrigger ketika game over

    public GameObject enemyPrefab;

    public GameObject templateFloatingText;

    void Awake() { instance = this; }

    void Start()
    {
        // Pastikan game tetap jalan meskipun window kehilangan fokus
        Application.runInBackground = true;
        PlayerController.isGameOverActive = false;
        StartCoroutine(StartingRoutine()); // Intro 5 detik
        SpawnEnemies(); // Munculkan musuh
    }

    IEnumerator StartingRoutine()
    {
        // Selama intro 5 detik, jangan bolehkan player gerak
        player.canMove = false;

        // Matikan dulu teks intro
        if (introCountdownText != null) introCountdownText.gameObject.SetActive(false);
        float currentSFXVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f); // Ambil volume SFX

        // Mainkan suara pertama 4 detik, tunggu 4 detik
        if (startSound1 != null && sfxAudioSource != null) sfxAudioSource.PlayOneShot(startSound1, currentSFXVol);
        yield return new WaitForSeconds(4f);

        // Mainkan suara kedua, tunggu 1 detik lagi
        if (startSound2 != null && sfxAudioSource != null) sfxAudioSource.PlayOneShot(startSound2, currentSFXVol);
        
        if (introCountdownText != null)
        {
            introCountdownText.gameObject.SetActive(true);
            // Selama 1 detik itu, juga mainkan animasi teks intro
            yield return StartCoroutine(AnimateGoTextRoutine(1f));
        }

        float targetBGMVol = PlayerPrefs.GetFloat("BGMVolume", 0.75f);

        if (bgmNormal != null)
        {
            bgmNormal.time = 0f;         
            bgmNormal.volume = targetBGMVol; 
            bgmNormal.Play();
        }
        if (bgmMuffled != null)
        {
            bgmMuffled.time = 0f;        
            bgmMuffled.volume = 0f;     
            bgmMuffled.Play();
        }

        // Baru boleh gerak sekarang
        player.canMove = true;
        if (gameOverManager != null) gameOverManager.StartSurvivalTimer(); // Hitung waktu game
        if (introCountdownText != null) introCountdownText.gameObject.SetActive(false); // Matikan teks intro
    }

    void SpawnEnemies()
    {
        List<int> quadrants = new List<int> { 1, 2, 3, 4 }; // Ada 4 kuadran

        // Acak menggunakan Fisher-Yates Shuffle
        for (int i = quadrants.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = quadrants[i];
            quadrants[i] = quadrants[randomIndex];
            quadrants[randomIndex] = temp;
        }

        for (int i = 0; i < 3; i++) // Munculkan prefab clone musuh 3 kali
        {
            int chosenQuadrant = quadrants[i];
            float viewportX = 0f;
            float viewportY = 0f;

            // Titik tengah koordinat viewport berdasarkan kuadran yg didapat
            switch (chosenQuadrant)
            {
                case 1: // kanan atas
                    viewportX = 0.85f;
                    viewportY = 0.85f;
                    break;
                case 2: // kiri atas
                    viewportX = 0.15f;
                    viewportY = 0.85f;
                    break;
                case 3: // kiri bawah
                    viewportX = 0.15f;
                    viewportY = 0.15f;
                    break;
                case 4: // kanan bawah
                    viewportX = 0.85f;
                    viewportY = 0.15f;
                    break;
            }

            Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(viewportX, viewportY, 10f));
            
            // Posisi spawn digeser sedikit secara acak agar tidak keluar layar
            spawnPos.x += Random.Range(-0.3f, 0.3f);
            spawnPos.y += Random.Range(-0.3f, 0.3f);
            spawnPos.z = 0;

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    public void CallGameOver()
    {
        // Skala waktu dinormalkan
        Time.timeScale = 1f;

        // Semua getaran off
        if (WorldShake.instance != null)
        {
            WorldShake.instance.StopAllCoroutines();
            WorldShake.instance.transform.localPosition = Vector3.zero;
        }
        
        Lowpass(true);

        // Player hilang
        if (player != null)
        {
            player.canMove = false;
            PlayerController.isGameOverActive = true;

            ParticleSystem dust = player.GetComponentInChildren<ParticleSystem>();
            if (dust != null)
            {
                dust.Stop();
                dust.Clear(); // Bersihkan partikel yang sedang melayang di layar
                var emission = dust.emission;
                emission.enabled = false;
            }

            // Hilangkan sprite player agar tampak lenyap
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
            Collider2D col = player.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // Game Over UI
        if (gameOverManager != null)
        {
            int finalScore = ScoreManager.instance != null ? ScoreManager.instance.GetScore() : 0;
            int totalCrystals = SpawnerManager.instance != null ? SpawnerManager.instance.GetCrystals() : 0;
            gameOverManager.TriggerGameOver(finalScore, totalCrystals);
        }

        if (ItemUIManager.instance != null) ItemUIManager.instance.ForceHideUIBox();
    }

    // Animasi teks EUYYY! yang membesar lalu hilang
    IEnumerator AnimateGoTextRoutine(float duration)
    {
        float elapsed = 0f;
        introCountdownText.text = "EUYYY!";
        
        // Atur skala awal dari 0 dan warna solid penuh
        introCountdownText.transform.localScale = Vector3.zero;
        Color originalColor = introCountdownText.color;
        introCountdownText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Perbesar skala hingga 1.8x
            float currentScale = Mathf.Lerp(0f, 1.8f, progress);
            introCountdownText.transform.localScale = new Vector3(currentScale, currentScale, 1f);

            // Mulai memudar perlahan setelah 0.5 detik
            if (progress > 0.5f)
            {
                float fadeProgress = (progress - 0.5f) / 0.5f;
                float alpha = Mathf.Lerp(1f, 0f, fadeProgress);
                introCountdownText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            yield return null;
        }
    }
    public void ShowFloatingTextAt(Vector3 worldPos, int pointValue)
    {
        if (templateFloatingText != null && templateFloatingText.transform.parent != null)
        {
            GameObject cloneObj = Instantiate(templateFloatingText, templateFloatingText.transform.parent);
            cloneObj.SetActive(true);
            
            TMP_Text tmp = cloneObj.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = "+" + pointValue;

            RectTransform canvasRect = templateFloatingText.transform.parent.GetComponent<RectTransform>();
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out localPoint))
            {
                StartCoroutine(FloatTextRoutine(cloneObj, localPoint));
            }
        }
    }

    IEnumerator FloatTextRoutine(GameObject txtObj, Vector2 startAnchoredPos)
    {
        float elapsed = 0f;
        float duration = 0.8f;
        RectTransform rect = txtObj.GetComponent<RectTransform>();
        TMP_Text txt = txtObj.GetComponent<TMP_Text>();
        Color startColor = txt != null ? txt.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            if (rect != null)
            {
                rect.anchoredPosition = startAnchoredPos + new Vector2(0, t * 80f);
            }

            if (txt != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                txt.color = c;
            }

            yield return null;
        }
        Destroy(txtObj);
    }

    public void Lowpass(bool isGameOverActive)
    {
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 0.75f);

        if (isGameOverActive)
        {
            // Suara normal dimatikan, suara teredam dinyalakan
            if (bgmNormal != null) bgmNormal.volume = 0f;
            if (bgmMuffled != null) bgmMuffled.volume = bgmVol;
        }
        else
        {
            // Normal menyala kembali, teredam dimatikan
            if (bgmNormal != null) bgmNormal.volume = bgmVol;
            if (bgmMuffled != null) bgmMuffled.volume = 0f;
        }
    }
}