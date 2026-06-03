using UnityEngine;
using System.Collections;

public class ItemObject : MonoBehaviour
{
    [Tooltip("0:Heal, 1:x2, 2:Slow, 3:Freeze, 4:Bonus, 5:Damage, 6:Half, 7:Fast, 8:Cloak, 9:Curse")]
    public int itemType; 
    public float lifetime = 5f
    ;
    [Header("Visual Effects")]
    public GameObject explosionParticlePrefab; 
    public AudioClip itemPickupSound;// Tarik Prefab partikel kristal kamu ke sini via Inspector

    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;
    private bool isCollected = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Mulai efek Fade In saat item muncul
        StartCoroutine(FadeRoutine(0f, 1f, 0.4f));
        // Mulai hitung mundur Lifetime hancur otomatis
        StartCoroutine(LifetimeRoutine());
    }

    IEnumerator LifetimeRoutine()
    {
        // Menunggu murni berdasarkan variabel lifetime (5 detik)
        yield return new WaitForSeconds(lifetime - 0.5f);
        
        if (!isCollected)
        {
            // Efek memudar jika tidak diambil player
            yield return StartCoroutine(FadeRoutine(1f, 0f, 0.5f));
            
            // JIKA HANGUS ALAMI: Mulai hitung mundur 15-20 detik untuk item berikutnya
            if (ItemSpawner.instance != null) 
            {
                ItemSpawner.instance.PlanNextSpawn();
            }
            
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            // Jalankan sekuens pengambilan lewat Coroutine agar urutan frame aman dan item PASTI hilang
            StartCoroutine(CollectSequenceRoutine());
        }
    }

    IEnumerator CollectSequenceRoutine()
    {
        isCollected = true;
        
        // Matikan collider dan sprite secara instan agar tidak bisa ditabrak dua kali dan langsung hilang dari pandangan
        if (itemCollider != null) itemCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.color = new Color(0, 0, 0, 0);

        // 1. Lahirkan Efek Ledakan (Explosion Particle reuse dari punya kristal)
        if (explosionParticlePrefab != null)
        {
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
        }

        // 2. Mainkan Efek Suara SFX
        if (itemPickupSound != null && GameManager.instance != null && GameManager.instance.sfxAudioSource != null)
        {
            // Ambil data kekuatan suara SFX murni dari preferensi slider pemain
            float currentSFXVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

            // Paksa fungsi PlayOneShot memutar suara dengan tingkat kekerasan sesuai slider!
            GameManager.instance.sfxAudioSource.PlayOneShot(itemPickupSound, currentSFXVol);
        }

        // 3. Picu Efek Mekanik Item ke Player/Musuh
        if (ItemEffectManager.instance != null)
        {
            ItemEffectManager.instance.TriggerItemEffect(itemType);
        }

        // Beri jeda 1 frame mikro agar pemicuan efek selesai diproses secara bersih oleh Unity
        yield return null;

        // 4. Jalankan siklus spawn item berikutnya secara aman
        if (ItemSpawner.instance != null)
        {
            ItemSpawner.instance.PlanNextSpawn();
        }

        // 5. Hancurkan objek item sepenuhnya dari memori game
        Destroy(gameObject);
    }

    IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
    {
        if (spriteRenderer == null) yield break;
        
        float elapsed = 0f;
        Color c = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        spriteRenderer.color = new Color(c.r, c.g, c.b, endAlpha);
    }
}