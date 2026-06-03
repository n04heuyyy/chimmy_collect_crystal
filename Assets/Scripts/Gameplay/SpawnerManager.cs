using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager instance { get; private set; }
    private int crystalCount = 0;

    [System.Serializable]
    public class SpawnSettings
    {
        public GameObject gemPrefab; // Kristal disimpan dalam prefab
        public float minWait = 2f; // Durasi default 2-4 detik untuk muncul
        public float maxWait = 4f;
    }

    public SpawnSettings smallGem; 
    public SpawnSettings mediumGem;
    public SpawnSettings largeGem;

    private Dictionary<string, bool> isOccupied = new Dictionary<string, bool>();
    private string currentMode;

    void Awake()
    {
        // Inisialisasi Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentMode = PlayerPrefs.GetString("GameMode", "classic");

        // Status awal tidak ada kristal yang muncul
        isOccupied["Small"] = false;
        isOccupied["Medium"] = false;
        isOccupied["Large"] = false;
        // Jalankan delay intro game
        StartCoroutine(SpawnDelay());
    }

    IEnumerator SpawnDelay()
    {
        // Tunggu 5 detik intro
        yield return new WaitForSeconds(5f);

        // Lalu semua rutin spawn dimulai untuk semua tipe
        StartCoroutine(SpawnRoutine(smallGem, "Small"));
        StartCoroutine(SpawnRoutine(mediumGem, "Medium"));
        StartCoroutine(SpawnRoutine(largeGem, "Large"));
    }

    IEnumerator SpawnRoutine(SpawnSettings settings, string typeKey)
    {
        while (true)
        {
            float min = settings.minWait;
            float max = settings.maxWait;

            // Mode unique (ccc advanced) muncul 2 kali lebih sering
            if (currentMode == "unique")
            {
                min /= 2f;
                max /= 2f;
            }

            float waitTime = Random.Range(min, max); // Acak durasi akan muncul
            yield return new WaitForSeconds(waitTime);

            // Mode classic hanya boleh 1 kristal per tipe yang muncul di layar
            // Mode unique boleh beberapa kristal per tipe sekaligus, jadi abaikan aturan khusus
            if (currentMode == "unique")
            {
                yield return new WaitForSeconds (waitTime); // Cukup tunggu wait time acak lalu muncul
                Spawn(settings, typeKey);
            } 
            else
            {
                if (!isOccupied[typeKey])
                {
                    yield return new WaitForSeconds (waitTime);
                    // Untuk muncul, tipe itu harus kosong di layar
                    if (!isOccupied[typeKey]) Spawn(settings, typeKey);
                }
                else
                {
                    // Jika layar masih terisi, beri jeda pendek sebelum cek lagi, agar no infinite loop
                    yield return new WaitForSeconds (0.5f);
                }
            } 
        }
    }

    void Spawn(SpawnSettings settings, string typeKey)
    {
        Vector3 spawnPos = GetRandomScreenPosition(); // Ada posisi area muncul
        GameObject gem = Instantiate(settings.gemPrefab, spawnPos, Quaternion.identity);
        
        // Tandai bahwa tipe ini sedang ada di layar (muntuk classic rule)
        isOccupied[typeKey] = true;

        // Berikan referensi ke script Item agar bisa melapor saat hancur
        Crystal itemScript = gem.GetComponent<Crystal>();
        if (itemScript != null)
        {
            itemScript.SetManager(this, typeKey);
            // Kristal ada lifetime, mode Unique lebih lama
            itemScript.lifetime = (currentMode == "unique") ? 8f : 3f;
        }
    }

    public void AddCrystalCount()
    {
        crystalCount++; // Semua tipe kristal dihitung 1
    }

    // GameManager ambil jumlah kristal untuk ditunjukkan oleh gameover manager
    public int GetCrystals()
    {
        return crystalCount;
    }

    public void NotifyItemDestroyed(string typeKey) // Beritahu kalau item diambil dan tidak di layar
    {
        if (isOccupied.ContainsKey(typeKey))
        {
            isOccupied[typeKey] = false;
        }
    }

    Vector3 GetRandomScreenPosition() // Buat batas area muncul kristal
    {
        Camera cam = Camera.main;
        float x = Random.Range(0.1f, 0.9f);
        float y = Random.Range(0.1f, 0.8f);
        
        Vector3 pos = cam.ViewportToWorldPoint(new Vector3(x, y, 10f));
        pos.z = 0f; 
        return pos;
    }
}