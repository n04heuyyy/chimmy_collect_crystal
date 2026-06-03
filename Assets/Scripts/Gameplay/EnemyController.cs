using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public float baseSpeed = 3f;
    public float currentSpeed; // Kecepatan saat ini setelah ditambah bonus
    public static float speedBonus = 0f; // Bonus global untuk semua musuh
    public float fadeDuration = 3f;

    private Vector2 direction;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Camera mainCam;
    private bool isReady = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
        currentSpeed = baseSpeed;
        speedBonus = 0f; // Reset speed bonus ke 0
        rb = GetComponent<Rigidbody2D>();
        
        // Set transparan di awal
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);

        // Kuadran ini untuk membantu acak arah
        float signX = (Random.value > 0.5f) ? 1f : -1f;
        float signY = (Random.value > 0.5f) ? 1f : -1f;

        // Acak sudut arah gerak musuh dengan batas 20-70 derajat
        // Agar musuh tidak bergerak mendekati lurus
        float randomAngleDeg = Random.Range(20f, 70f);
        
        // Ubah derajat ke radian untuk fungsi Trigonometri Unity
        float angleRad = randomAngleDeg * Mathf.Deg2Rad;

        // Hitung arah X dan Y berdasarkan sudut, lalu kalikan dengan kuadrannya
        float directionX = Mathf.Cos(angleRad) * signX;
        float directionY = Mathf.Sin(angleRad) * signY;
        direction = new Vector2(directionX, directionY).normalized;

        // Paksa flip agar muncul menghadap arah gerak yang sesuai
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }

        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = elapsed / fadeDuration;
                sr.color = c;
            }
            yield return null;
        }
        isReady = true;
    }

    void Update()
    {
        if (!isReady) return;
        
        // Hitung penambahan kecepatan global, lalu update kecepatan musuh
        speedBonus += (1f / 80f) * Time.deltaTime;
        currentSpeed = baseSpeed + speedBonus;

        // Jika item Freeze aktif, hentikan di tempat
        float finalSpeedMultiplier = 1f;
        if (ItemEffectManager.instance != null)
        {
            if (ItemEffectManager.instance.isEnemiesFrozen)
            {
                // Lewati fungsi pergerakan
                return; 
            }
            // Ambil pengali kecepatan musuh (default 1, bisa diubah effect item)
            finalSpeedMultiplier = ItemEffectManager.instance.enemySpeedMultiplier;
        }

        // Gerakkan musuh menggunakan currentSpeed
        float calculatedSpeed = currentSpeed * finalSpeedMultiplier;
        transform.Translate(direction * calculatedSpeed * Time.deltaTime);

        // Deteksi batas layar Viewport (0 sampai 1) dan ada margin
        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);
        float marginX = 0.07f; 
        float marginY = 0.09f;

        // Musuh memantul kiri/kanan akan flip
        if (viewPos.x <= marginX) // Terkena batas kiri
        {
            direction.x = Mathf.Abs(direction.x); // Paksa ke kanan
            FixPosition(marginX, viewPos.y); // Kembalikan ke dalam layar
            Flip();
        }
        else if (viewPos.x >= 1f - marginX) // Sebaliknya
        {
            direction.x = -Mathf.Abs(direction.x); // Paksa ke kiri
            FixPosition(1f - marginX, viewPos.y);
            Flip();
        }

        // Musuh memantul atas/bawah tidak flip
        if (viewPos.y <= marginY) // Terkena batas bawah
        {
            direction.y = Mathf.Abs(direction.y); // Paksa ke atas
            FixPosition(viewPos.x, marginY);
        }
        else if (viewPos.y >= 1f - marginY) //Sebaliknya
        {
            direction.y = -Mathf.Abs(direction.y); // Paksa ke bawah
            FixPosition(viewPos.x, 1f - marginY);
        }
    }

    void FixPosition(float xViewport, float yViewport) // Sesuaikan posisi musuh ketika memantul
    {
        Vector3 correctedPos = mainCam.ViewportToWorldPoint(new Vector3(xViewport, yViewport, 10f));
        correctedPos.z = 0;
        transform.position = correctedPos;
    }
    void Flip() // Balik sprite jika memantul kiri/kanan
    {
        if (direction.x > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }
}