using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Toggle OHKOToggle;
    public Image fadeImage; // Image hitam untuk fade ganti scene
    public float fadeDuration = 0.8f;
    public AudioSource bgmSource;
    
    [Header("Submenu Panels")]
    public RectTransform leaderboardPanel;
    public RectTransform helpGuidePanel;
    public RectTransform optionsPanel;

    [Header("Leaderboard Pages Panels")]
    public GameObject classicTabPanel; // Panel untuk Classic & Classic OHKO
    public GameObject uniqueTabPanel;  // Panel untuk Unique & Unique OHKO

    [Header("Leaderboard List Text")]
    public TMP_Text leftColumnText;  // Untuk menampilkan leaderboard tanpa OHKO on
    public TMP_Text rightColumnText; // Untuk menampilkan leaderboard dengan OHKO on

    [Header("Animation Settings")]
    public float positionX = 0f; // Koordinat sembunyi di atas luar layar Canvas
    public float hidePositionY = 1080f;
    public float targetShowY = 0f;    // Posisi Y tempat muncul menjadi 0 (X tetap 0)
    public float animDuration = 0.5f; // Durasi animasi panel

    private bool isTransitioning = false;
    private Dictionary<RectTransform, Coroutine> activePanelCoroutines = new Dictionary<RectTransform, Coroutine>();

    void Start()
    {
        ShowClassicTab(); // Tab Classic sebagai default

        if (fadeImage != null)
        {
            StartCoroutine(ToMenuRoutine());
        }
    
        // Sembunyikan semua panel 
        InitializePanelState(leaderboardPanel);
        InitializePanelState(helpGuidePanel);
        InitializePanelState(optionsPanel);
    }

    void InitializePanelState(RectTransform panel)
    {
        if (panel != null) // Letakkan panel di posisi yang ditentukan, lalu nonaktifkan
        {
            panel.anchoredPosition = new Vector2(positionX, hidePositionY);
            panel.gameObject.SetActive(false);
        }
    }

    // Jalankan transisi masuk gameplay scene sambil membawa mode game
    public void PlayClassicMode() 
    {
        if (!isTransitioning) StartCoroutine(ToGameplayRoutine("classic"));
    }

    public void PlayUniqueMode() 
    {
        if (!isTransitioning) StartCoroutine(ToGameplayRoutine("unique"));
    }

    IEnumerator ToGameplayRoutine(string mode) // Simpan data game dan fade layar menjadi hitam
    {
        isTransitioning = true;

        // Simpan data mode game
        PlayerPrefs.SetString("GameMode", mode);
        PlayerPrefs.SetInt("OneHitKO", OHKOToggle.isOn ? 1 : 0);

        fadeImage.raycastTarget = true; // Blokir klik

        float duration = 1.0f;
        float elapsed = 0;
        float startVolume = bgmSource.volume;
        while (elapsed < duration) // Ubah alpha gradual menjadi 1 (terlihat)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / duration;
            float alpha = Mathf.Lerp(0f, 1f, ratio);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);

            if (bgmSource != null) // Juga fade out musik
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0, ratio);
            }
            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        SceneManager.LoadScene("CCC Gameplay");
    }

    IEnumerator ToMenuRoutine() // Fade layar menjadi terlihat
    {
        fadeImage.raycastTarget = true;
        float elapsed = 0f;
        while (elapsed < fadeDuration) // Ubah alpha gradual menjadi 0 (transparan)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration); 
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false; 
    }

    // Function tombol buka/tutup panel, membawa titik target geser
    public void OpenLeaderboard()
    {
        if (leaderboardPanel == null) return;
        if (classicTabPanel.activeSelf) ShowClassicTab(); else ShowUniqueTab();

        TriggerPanelAnimation(leaderboardPanel, positionX, targetShowY);
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel == null) return;
        TriggerPanelAnimation(leaderboardPanel, positionX, hidePositionY);
    }

    public void OpenHelpGuide()
    {
        if (helpGuidePanel == null) return;
        TriggerPanelAnimation(helpGuidePanel, positionX, targetShowY);
    }

    public void CloseHelpGuide()
    {
        if (helpGuidePanel == null) return;
        TriggerPanelAnimation(helpGuidePanel, positionX, hidePositionY);
    }

    public void OpenOptions()
    {
        if (optionsPanel == null) return;
        TriggerPanelAnimation(optionsPanel, positionX, targetShowY);
    }

    public void CloseOptions()
    {
        if (optionsPanel == null) return;
        TriggerPanelAnimation(optionsPanel, positionX, hidePositionY);
    }

    private void TriggerPanelAnimation(RectTransform panel, float targetX, float targetY)
    {
        // Hentikan animasi panel yang berjalan untuk jaga2
        if (activePanelCoroutines.ContainsKey(panel) && activePanelCoroutines[panel] != null)
        {
            StopCoroutine(activePanelCoroutines[panel]);
        }
        // Jalankan animasi panel menuju titik yang ditentukan
        activePanelCoroutines[panel] = StartCoroutine(SlidePanelRoutine(panel, targetX, targetY));
    }

    IEnumerator SlidePanelRoutine(RectTransform panel, float targetX, float targetY)
    {
        float elapsed = 0f;
        Vector2 startPos = panel.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, targetY);

        // Aktifkan objek sebelum digeser ke layar
        if (targetY == targetShowY) panel.gameObject.SetActive(true);

        while (elapsed < animDuration) // Geser ke titik yang ditentukan
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panel.anchoredPosition = targetPos;

        // Jika panel meluncur kembali ke luar layar, matikan objek agar efisien
        if (targetY == hidePositionY) panel.gameObject.SetActive(false);
    }

    // Memunculkan subpanel leaderboard (ada 2 per mode, beda warna bg dan teks yang dimunculkan)
    public void ShowClassicTab()
    {
        classicTabPanel.SetActive(true);
        uniqueTabPanel.SetActive(false);
        
        RenderList("classic", leftColumnText);
        RenderList("classic_ohko", rightColumnText);
    }

    public void ShowUniqueTab()
    {
        classicTabPanel.SetActive(false);
        uniqueTabPanel.SetActive(true);
        
        RenderList("unique", leftColumnText);
        RenderList("unique_ohko", rightColumnText);
    }

    void RenderList(string modeKey, TMP_Text targetDisplay)
    {
        if (targetDisplay == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Buat judul leaderboard berdasarkan mode
        string displayTitle = "";

        switch (modeKey.ToLower())
        {
            case "classic": displayTitle = "Classic"; break;
            case "classic_ohko": displayTitle = "Classic OHKO"; break;
            case "unique": displayTitle = "CCC Advanced"; break;
            case "unique_ohko": displayTitle = "CCC Advanced OHKO"; break;
            default: displayTitle = modeKey.Replace("_", " ").ToUpper(); break;
        }

        // Tampilkan judul ke UI Leaderboard
        sb.AppendLine($"<size=130%><b>{displayTitle} Mode</b></size>\n");
        for (int i = 0; i < 10; i++)
        {
            // Read data dari memori, menggunakan .ToLower() agar tidak salah ambil
            string pName = PlayerPrefs.GetString($"{modeKey.ToLower()}_Name_{i}", "---");
            int pScore = PlayerPrefs.GetInt($"{modeKey.ToLower()}_Score_{i}", 0);
            int pTimeSeconds = PlayerPrefs.GetInt($"{modeKey.ToLower()}_Time_{i}", 0);

            // Format detik dibuat menjadi MM:SS
            string timeFormatted = "00:00";
            if (pName != "---")
            {
                int mins = pTimeSeconds / 60;
                int secs = pTimeSeconds % 60;
                timeFormatted = string.Format("{0:00}:{1:00}", mins, secs);
            }
            
            string rankNum = (i + 1) < 10 ? "0" + (i + 1) : (i + 1).ToString();
            
            // Tampilkan nama, skor, dan waktu (menggunakan PadRight untuk merapikan spasi kolom)
            string nameSegment = pName.PadRight(10, '.');
            string scoreSegment = pScore.ToString();
            
            sb.AppendLine($"{rankNum}. {nameSegment} {scoreSegment} ({timeFormatted})");
        }
        targetDisplay.text = sb.ToString();
    }

    public void ClearAllLeaderboards()
    {
        string[] modes = { "classic", "classic_ohko", "unique", "unique_ohko" };
        
        foreach (string mode in modes) // Hapus semua data
        {
            PlayerPrefs.DeleteKey($"{mode}_Score_Count");
            for (int i = 0; i < 10; i++)
            {
                PlayerPrefs.DeleteKey($"{mode}_Name_{i}");
                PlayerPrefs.DeleteKey($"{mode}_Score_{i}");
                PlayerPrefs.DeleteKey($"{mode}_Time_{i}");
            }
        }
        PlayerPrefs.Save();
        
        // Refresh tampilan yang sedang aktif
        if (classicTabPanel.activeSelf) ShowClassicTab();
        else ShowUniqueTab();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Mematikan Play Mode di Unity Editor
        #else
            Application.Quit(); // Menutup game saat sudah di-build (.exe / .apk)
            // Dalam mode web ini akan memunculkan pesan lol XD
        #endif
    }
}