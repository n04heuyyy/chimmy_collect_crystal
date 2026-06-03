using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance { get; private set; }

    [Header("Player Movement")]
    public float distanceFromCamera = 10f;
    public float padding = 1f;
    public bool canMove = false; // Player mulai dalam keadaan diam
    private Vector3 lastPosition;
    
    [Header("Player HP")]
    public float health = 20f;
    public float maxHealth = 20f;
    public float nextPopTime = 0f;
    public TextMeshProUGUI healthText;

    public LayerMask enemyLayer; // Layer enemy khusus musuh untuk trigger damage
    public float detectionRadius = 0.9f;

    private SpriteRenderer playerSR;
    private Camera mainCam;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        mainCam = Camera.main;
        playerSR = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
        string mode = PlayerPrefs.GetString("GameMode", "classic").ToLower();

        if (PlayerPrefs.GetInt("OneHitKO", 0) == 1) 
        maxHealth = 1f; // Sekali kena langsung mati di OHKO
        else
        {
            maxHealth = (mode == "unique") ? 40f : 20f; // Nyawa di classic 20, unique 40
        }
        health = maxHealth;
        UpdateHealthUI();
    }
    public int GetCurrentHP()
    {
        return Mathf.CeilToInt(health); // Ambil HP saat ini
    }

    void Update()
    {
        if (canMove && Mouse.current != null)
        {
            // 1. Ambil posisi mouse
            Vector2 inputPixelPos = Vector2.zero;
            bool hasInput = false;

            // Cek apakah ada touch di layar mobile
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                inputPixelPos = Touchscreen.current.primaryTouch.position.ReadValue();
                hasInput = true;
            }
            // Kalau tidak ada, cek apakah ada input mouse di pc
            else if (Mouse.current != null)
            {
                inputPixelPos = Mouse.current.position.ReadValue();
                hasInput = true;
            }

            if (hasInput)
            {
                Vector3 viewportPos = mainCam.ScreenToViewportPoint(new Vector3(inputPixelPos.x, inputPixelPos.y, distanceFromCamera));
                
                if (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
                {
                    Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(inputPixelPos.x, inputPixelPos.y, distanceFromCamera));

                    // Batas layar sprite bisa bergerak menggunakan clamping, diatur menggunakan padding
                    Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, distanceFromCamera));
                    Vector3 topRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, distanceFromCamera));

                    float clampedX = Mathf.Clamp(worldPos.x, bottomLeft.x + padding, topRight.x - padding);
                    float clampedY = Mathf.Clamp(worldPos.y, bottomLeft.y + padding, topRight.y - padding);
                    Vector3 targetPos = new Vector3(clampedX, clampedY, worldPos.z);

                    // Gerakkan player mengikuti kursor
                    float smoothness = 0.7f; // Semakin mendekati 1, semakin instan menempel ke kursor
                    transform.position = Vector3.Lerp(transform.position, targetPos, smoothness);
                    
                    // Update arah hadap kiri/kanan
                    UpdateFlip(targetPos);
                }
            }
        }

        // Buat deteksi damage selalu bekerja, bahkan jika pointer di luar layar
        bool isHittingEnemyBackup = CheckCollisionBackup();
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, enemyLayer);

        if (hit != null || isHittingEnemyBackup) ApplyDamage();
        else
        {
            // Pastikan sprite kembali ke semula jika tidak kerkena damage lagi atau dalam berkedip
            if (ItemEffectManager.instance == null || (!ItemEffectManager.instance.isPlayerInvincible && !ItemEffectManager.instance.isBlinkingEffectActive))
            {
                playerSR.color = Color.white;
            }
        }
    }

    // Balik sprite kiri/kanan tergantung arah gerak pointer
    void UpdateFlip(Vector3 targetPos)
    {
        if (targetPos.x > transform.position.x + 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else if (targetPos.x < transform.position.x - 0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    bool CheckCollisionBackup() // Hit detection ketika pointer static
    {
        float distance = Vector3.Distance(lastPosition, transform.position);
        bool isHittingEnemy = false;
        if (distance > 0)
        {
            Vector3 direction = (transform.position - lastPosition).normalized;
            // Radius deteksi musuh selama static
            RaycastHit2D[] hits = Physics2D.CircleCastAll(lastPosition, 0.9f, direction, distance);
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    isHittingEnemy = true; // Jika mengenai musuh, anggap kena
                    break;
                }
            }
            if (!isHittingEnemy) playerSR.color = Color.white; // Pitih ketika tidak kena
        }
        lastPosition = transform.position;
        return isHittingEnemy; // Kembalikan status tabrakan ke fungsi Update utama
    }

    public IEnumerator PopAnimation(Transform target, float scaleAmount) // Animasi pop kotak HP
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
    void ApplyDamage()
    {
        // Jika sedang kebal karena item, abaikan damage
        if (ItemEffectManager.instance != null && ItemEffectManager.instance.isPlayerInvincible)
        {
            playerSR.color = Color.white; // Kembalikan warna normal
            return;
        }

        health -= 20f * Time.deltaTime; // Nyawa berkurang sebanyak 20/detik
        playerSR.color = Color.red; // Selama terkena damage, ubah sprite jadi merah
        UpdateHealthUI();

        if (health <= 0) // Game over ketika HP 0
        {
            health = 0;
            if (GameManager.instance != null)GameManager.instance.CallGameOver();
        }
    }
    public void ModifyHP(int amount) // Tambah/kurang HP dari item
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, maxHealth);
        UpdateHealthUI();
        StartCoroutine(PopAnimation(healthText.transform.parent, 1.2f));

        if (health <= 0)
        {
            health = 0;
           if (GameManager.instance != null) GameManager.instance.CallGameOver();
        }
    }

    public void SetHPDirectly(int newHP) // Set nyawa langsung untuk item one hit curse
    {
        health = Mathf.Clamp(newHP, 0, maxHealth);
        UpdateHealthUI();
        StartCoroutine(PopAnimation(healthText.transform.parent, 1.2f));

        if (health <= 0)
        {
            if (GameManager.instance != null) GameManager.instance.CallGameOver();
        }
    }

    void UpdateHealthUI() // Ubah jumlah HP di UI dan jalankan animasi pop kotak UI
    {
        if (healthText == null) return;
        healthText.text = "HP: " + Mathf.CeilToInt(health).ToString();

        if (playerSR.color == Color.red && Time.time > nextPopTime)
        {
            nextPopTime = Time.time + 0.20f;
            StartCoroutine(PopAnimation(healthText.transform.parent, 1.2f));
        }
        healthText.color = (health <= 5) ? Color.red : Color.black; // Jika nyawa 5 ke bawah, buat merah
    }
}