using UnityEngine;
using System.Collections;

public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager instance { get; private set; }

    [Header("Current Status")]
    public float scoreMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;
    public bool isEnemiesFrozen = false;
    public bool isPlayerInvincible = false;

    private bool isOHKO = false;

    [Header("Effect Coroutine")]
    private Coroutine speedCoroutine, freezeCoroutine, multiplierCoroutine, curseCoroutine;
    public bool isBlinkingEffectActive = false; // Penanda barkedip
    private Coroutine playerBlinkCoroutine, enemyBlinkCoroutine;

    // Data nama dan icon item akan dikirim ke UI
    [System.Serializable]
    public struct ItemUIData
    {
        public string itemName;
        public Sprite itemIcon;
    }
    
    [Header("UI Data Config (Size 10)")]
    public ItemUIData[] itemUIConfigs; // Data Ui disimpan disini

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Cek apakah mode One-Hit KO sedang aktif, ada beberapa item yang beda efek
        isOHKO = PlayerPrefs.GetInt("OneHitKO", 0) == 1;
    }

    public void TriggerItemEffect(int itemType)
    {
        switch (itemType)
        {
            case 0: // Heal/Shield
                if (isOHKO) {
                    // Jika OHKO on, kebal selama 5 detik
                    if (playerBlinkCoroutine != null) StopCoroutine(playerBlinkCoroutine);
                    playerBlinkCoroutine = StartCoroutine(InvincibilityRoutine(5f));
                } else {
                // Jika OHKO off, tambah HP sebanyak 10, tampilkan sebagai efek instan
                    PlayerController.instance.ModifyHP(10);
                    StartCoroutine(NonOHKOHealEffectRoutine());
                }
                NotifyUIBox(0, isOHKO ? 5f : 3f);
                break;

            case 1: // x2 Multiplier, ubah multiplier skor menjadi 2 selama 10 detik
                if (multiplierCoroutine != null) StopCoroutine(multiplierCoroutine);
                multiplierCoroutine = StartCoroutine(ScoreMultiplierRoutine(2f, 10f, Color.green));
                NotifyUIBox(1, 10f);
                break;

            case 2: // Slow Enemies, ubah multiplier speed musuh menjadi 0.5 selama 10 detik
                if (speedCoroutine != null) StopCoroutine(speedCoroutine);
                speedCoroutine = StartCoroutine(EnemySpeedRoutine(0.5f, 10f, "slow"));
                NotifyUIBox(2, 10f);
                break;

            case 3: // Freeze Enemies, bekukan musuh dan buat aman disentuh
                if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
                freezeCoroutine = StartCoroutine(FreezeEnemiesRoutine(5f));
                NotifyUIBox(3, 5f);
                break;

            case 4: // Bonus Points, tambah skor 500
                if (ScoreManager.instance != null) ScoreManager.instance.AddScore(500);
                NotifyUIBox(4, 3f); // Instan: 3 detik
                break;

            case 5: // Instadamage
                 // Jika OHKO on atau nyawa 15 ke bawah, langsung mati
                if (isOHKO || PlayerController.instance.health <= 15) GameManager.instance.CallGameOver();
                else {
                    PlayerController.instance.ModifyHP(-15); // Jika tidak, kurangi HP sebanyak 15
                    StartCoroutine(InstadamageEffectRoutine());
                    NotifyUIBox(5, 3f);
                }
                break;

            case 6: // Half Multiplier, ubah multiplier skor menjadi 0.5 selama 10 detik
                if (multiplierCoroutine != null) StopCoroutine(multiplierCoroutine);
                multiplierCoroutine = StartCoroutine(ScoreMultiplierRoutine(0.5f, 10f, Color.Lerp(Color.red, Color.yellow, 0.5f)));
                NotifyUIBox(6, 10f);
                break;

            case 7: // Fast Enemies, ubah multiplier speed musuh menjadi 2 selama 10 detik
                if (speedCoroutine != null) StopCoroutine(speedCoroutine);
                speedCoroutine = StartCoroutine(EnemySpeedRoutine(1.5f, 10f, "fast"));
                NotifyUIBox(7, 10f);
                break;

            case 8: // Cloak Enemies, buat musuh hampir transparan (ubah alpha) selama 10 detik
                StartCoroutine(CloakEnemiesRoutine(10f));
                NotifyUIBox(8, 10f);
                break;

            case 9: // Onehit Curse
                if (isOHKO) GameManager.instance.CallGameOver(); // Jika OHKO, langsung mati
                else
                {
                    if (curseCoroutine != null) StopCoroutine(curseCoroutine); // Hentikan yang lama
                    curseCoroutine = StartCoroutine(OneHitCurseRoutine(10f)); // Set HP ke 1 selama 10 detik
                    NotifyUIBox(9, 10f);
                }
                break;
        }
    }

    void NotifyUIBox(int itemIndex, float duration) // Beritahu isi UI yang ditampilkan
    {
        string nameOfItem = itemUIConfigs[itemIndex].itemName;
        Sprite iconOfItem = itemUIConfigs[itemIndex].itemIcon;
        
        // Setelah dapat data item, tampilkan UI dari managerUI item
        if (ItemUIManager.instance != null)
        {
            ItemUIManager.instance.ShowItemBox(nameOfItem, iconOfItem, duration);
        }
    }

    // Routine untuk mengatur efek item
    IEnumerator InvincibilityRoutine(float duration) // Player kebal dan kuning berkedip lambat, 5s
    {
        isPlayerInvincible = true; // Aktifkan kebal
        isBlinkingEffectActive = true; // Aktifkan berkedip
        float elapsed = 0f;
        SpriteRenderer playerSR = PlayerController.instance.GetComponent<SpriteRenderer>();
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (playerSR != null)
            {
                // Kalikan 4f untuk denyut lambat (PingPOng untuk menghaluskan kedipan)
                float pingPong = Mathf.PingPong(Time.time * 4f, 1f); 
                playerSR.color = Color.Lerp(Color.white, Color.yellow, pingPong);
            }
            yield return null;
        }
        
        if (playerSR != null) playerSR.color = Color.white; // Kembalikan warna asli sprite
        isPlayerInvincible = false; // Nonaktifkan semua efek
        isBlinkingEffectActive = false;
    }

    public IEnumerator NonOHKOHealEffectRoutine() // Player hijau sebentar (1 detik dengan fade out)
    {
        isBlinkingEffectActive = true;
        SpriteRenderer playerSR = PlayerController.instance.GetComponent<SpriteRenderer>();
    
        float elapsed = 0f;
        float duration = 1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // Perlahan dari hijau ke putih
            if (playerSR != null)
                playerSR.color = Color.Lerp(Color.green, Color.white, elapsed / duration);
            yield return null;
        }
        isBlinkingEffectActive = false;
    }

    IEnumerator ScoreMultiplierRoutine(float multiplier, float duration, Color blinkColor)
    {
        scoreMultiplier = multiplier; // Aktifkan pengali skor
        isBlinkingEffectActive = true;

        float elapsed = 0f;
        SpriteRenderer playerSR = PlayerController.instance.GetComponent<SpriteRenderer>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (playerSR != null)
            {
                // Denyut kecepatan standar (dikali 8f), warna diatur item diatas
                // Player berkedip hijau di x2 mult, oranye di half mult, 10s
                float pingPong = Mathf.PingPong(Time.time * 8f, 1f);
                playerSR.color = Color.Lerp(Color.white, blinkColor, pingPong);
            }
            yield return null;
        }
        if (playerSR != null) playerSR.color = Color.white;
        scoreMultiplier = 1f;
        isBlinkingEffectActive = false;
    }

    public IEnumerator InstadamageEffectRoutine() // Player merah sebentar (1 detik dengan fade out)
    {
        isBlinkingEffectActive = true;
        SpriteRenderer playerSR = PlayerController.instance.GetComponent<SpriteRenderer>();
        
        float elapsed = 0f;
        float duration = 1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (playerSR != null)
                playerSR.color = Color.Lerp(Color.red, Color.white, elapsed / duration);
            yield return null;
        }
        isBlinkingEffectActive = false;
    }

    IEnumerator OneHitCurseRoutine(float duration) // Set HP player ke 1, dan merah berkedip, 10s
    {
        float originalHP = PlayerController.instance.health; // Simpan HP asli dulu
        PlayerController.instance.SetHPDirectly(1); // Set HP ke 1
        isBlinkingEffectActive = true;

        float elapsed = 0f;
        SpriteRenderer playerSR = PlayerController.instance.GetComponent<SpriteRenderer>();
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (playerSR != null)
            {
                float pingPong = Mathf.PingPong(Time.time * 8f, 1f);
                playerSR.color = Color.Lerp(Color.white, Color.red, pingPong);
            }
            yield return null;
        }

        if (playerSR != null) playerSR.color = Color.white;
        isBlinkingEffectActive = false;

        if (PlayerController.instance.health > 0) // Jika tidak mati, kembalikan HP asli
        {
            PlayerController.instance.SetHPDirectly(Mathf.CeilToInt(originalHP));
        }
    }

    IEnumerator EnemySpeedRoutine(float multiplier, float duration, string modeType)
    {
        enemySpeedMultiplier = multiplier; // Aktifkan pengali kecepatan musuh

       float elapsed = 0f;
        // Tentukan kecepatan denyutan: fast (8f) atau slow (2f)\
        // Ini tergantung item slow/fast enemies, dan berkedip kuning secepat itu
        float pulseSpeed = (modeType == "fast") ? 8f : 2f; 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pingPong = Mathf.PingPong(Time.time * pulseSpeed, 1f);

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies) // Diterapkan di objek dengan tag Enemy
            {
                SpriteRenderer esr = enemy.GetComponent<SpriteRenderer>();
                if (esr != null)
                {
                    // Transisi halus ke warna Kuning
                    esr.color = Color.Lerp(Color.white, Color.yellow, pingPong);
                }
            }
            yield return null;
        }

        GameObject[] finalEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in finalEnemies)
        {
            SpriteRenderer esr = enemy.GetComponent<SpriteRenderer>();
            if (esr != null) esr.color = Color.white;
        }

        enemySpeedMultiplier = 1f;
    }

    IEnumerator FreezeEnemiesRoutine(float duration) // Bekukan musuh, buat biru
    {
        isEnemiesFrozen = true; // Aktifkan status membeku agar pergerakan musuh dinonaktifkan

        // Paksa semua musuh menjadi biru
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            SpriteRenderer esr = enemy.GetComponent<SpriteRenderer>();
            if (esr != null) esr.color = Color.blue;
            enemy.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        yield return new WaitForSeconds(duration);

        GameObject[] finalEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in finalEnemies)
        {
            SpriteRenderer esr = enemy.GetComponent<SpriteRenderer>();
            if (esr != null) esr.color = Color.white;
            enemy.layer = LayerMask.NameToLayer("Default");
        }
        isEnemiesFrozen = false;
    }

    IEnumerator CloakEnemiesRoutine(float duration)
    {
        SetEnemiesAlpha(15f / 255f); // Ubah alpha hingga hampir transparan
        yield return new WaitForSeconds(duration);
        SetEnemiesAlpha(1f);
    }

    void SetEnemiesAlpha(float alphaValue) // Digunakan untuk membuat musuh hampir transparan (cloak)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, alphaValue);
            }
        }
    }
}