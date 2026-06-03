using UnityEngine;
using System.Collections;

public class Crystal : MonoBehaviour
{
    [Header("Crystal Attributes")]
    public Sprite[] variants;
    public int points = 10; // base, ada 3 varian berbeda skor
    public float lifetime = 5f;
    public float fadeSpeed = 2f;
    private bool isCollected = false; // Penanda sudah diambil

    private SpawnerManager manager;
    private Coroutine destroyCoroutine;
    private string myType;
    private SpriteRenderer sr;

    [Header("Visual & Audio")]
    public GameObject explosionEffect;
    public AudioClip collectSound;

    void Start()
    {
        destroyCoroutine = StartCoroutine(LifetimeRoutine());
        sr = GetComponent<SpriteRenderer>();

        // Pilih varian sprite (berbeda ukuran)
        if (variants.Length > 0)
            sr.sprite = variants[Random.Range(0, variants.Length)];

        // Mulai transparan, lalu fade in
        StartCoroutine(FadeIn());

        // Hancur otomatis setelah durasi tertentu
        Destroy(gameObject, lifetime);
    }

    IEnumerator FadeIn()
    {
        float alpha = 0;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        while (alpha < 1f) // Perlahan buat terlihat (ubah alpha)
        {
            alpha += Time.deltaTime * fadeSpeed;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true; // Kunci agar tidak bisa masuk lagi

            if (ItemEffectManager.instance != null)
            {
                // Kalikan poin asli dengan multiplier dari item effect
                points = Mathf.RoundToInt(points * ItemEffectManager.instance.scoreMultiplier);
            }
            if (ScoreManager.instance != null)
            ScoreManager.instance.AddScore(points); // Tambahkan ke skor total

            HandleEffects(); 
            // Lalu spawner menghitung jumlah kristal diambil
            if (SpawnerManager.instance != null) SpawnerManager.instance.AddCrystalCount();

            Destroy(gameObject);
        }
    }

    void HandleEffects()
    {
        if (explosionEffect != null)
        {
            // Munculkan partikel ledakan 
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (collectSound != null && GameManager.instance != null && GameManager.instance.sfxAudioSource != null)
        {
            // Ambil volume SFX dari slider di menu dan putar
            float currentSFXVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            GameManager.instance.sfxAudioSource.PlayOneShot(collectSound, currentSFXVol);
        }
    }
    IEnumerator LifetimeRoutine()
    {
        
        yield return new WaitForSeconds(lifetime - 1f);
        float fadeDuration = 1f;
        float elapsed = 0;
        Color startColor = sr.color;

        while (elapsed < fadeDuration) // Ketika lifetime item mau habis, fade out
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject); // dan hapus
    }
    public void SetManager(SpawnerManager m, string type) // Beritahu tipe item ke spawner
    {
        manager = m;
        myType= type;
    }

    void OnDestroy() 
    {
        // Lapor ke spawner agar slot tipe ini kosong kembali
        // Mode classic hanya boleh 1 kristal per tipe yang muncul
        if (manager != null) manager.NotifyItemDestroyed(myType);
    }
}