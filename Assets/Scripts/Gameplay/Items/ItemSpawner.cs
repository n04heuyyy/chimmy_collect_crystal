using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner instance { get; private set; }

    public GameObject[] itemPrefabs; // Ada 10 variasi prefab item

    private bool isUniqueMode = false; // Item hanya dimunculkan di mode unique
    private bool isWaitingForNextSpawn = false; // Item muncul setelah diambil atau lifetime habis

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        string mode = PlayerPrefs.GetString("GameMode", "classic").ToLower();
        isUniqueMode = mode == "unique";

        if (isUniqueMode)
        {
            // Mulai buat kemunculan pertama
            PlanNextSpawn();
        }
    }

    public void PlanNextSpawn()
    {
        // Cegah double coroutine berjalan bersamaan
        if (isWaitingForNextSpawn) return;
        StartCoroutine(SpawnCooldownRoutine());
    }

    IEnumerator SpawnCooldownRoutine()
    {
        isWaitingForNextSpawn = true;

        // Waktu muncul item antara 15-20 detik
        float randomCooldown = Random.Range(15f, 20f);
        yield return new WaitForSeconds(randomCooldown);

        SpawnRandomItem();
        isWaitingForNextSpawn = false;
    }

    void SpawnRandomItem()
    {
        if (itemPrefabs.Length == 0) return;

        // Pilih jenis item secara acak dari 10 tipe (0 sampai 9)
        int randomIndex = Random.Range(0, itemPrefabs.Length);

        // Tentukan posisi acak di dalam layar (ada batasnya)
        float randomX = Random.Range(0.15f, 0.85f);
        float randomY = Random.Range(0.15f, 0.85f);
        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, 10f));
        spawnPos.z = 0f; // Kunci di koordinat 2D

        // 3. Munculkan item menggunakan instantiate
        Instantiate(itemPrefabs[randomIndex], spawnPos, Quaternion.identity);
    }
}