using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BGManager : MonoBehaviour
{
    public Image imageLayer1; // Menggunakan 2 buah layer image yang bergantian berubah
    public Image imageLayer2;
    public int totalImages = 17; // Ada 17 gambar bernama bg_0, dst, akan diambil angka indeksnya

    public float changeTime = 5f; // Berganti setiap 5 detik
    public float fadeDuration = 1.5f; // dengan fade 1.5 detik
    

    private int currentIndex = 0; // Indeks ini mengambil background dari namanya (sudah diformat)
    private bool isLayer1Active = true;

    void Start()
    {
        // Ambil acak 1 background lalu muat ke layer 1
        // Sedangkan layer 2 dibuat kosong
        currentIndex = Random.Range(0, totalImages);
        imageLayer1.sprite = LoadSprite(currentIndex);
        imageLayer1.color = Color.white;
        imageLayer2.color = new Color(1, 1, 1, 0);

        StartCoroutine(BackgroundCycle());
    }

    IEnumerator BackgroundCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(changeTime); // Tunggu 5 detik baru ganti

            // Ambil indeks dari Start, selama dapatnya sama dengan yang sekarang diacak
            // Sehingga dijamin tidak mendapat gambar yang sama
            int nextIndex = currentIndex;
            while (nextIndex == currentIndex)
            {
                nextIndex = Random.Range(0, totalImages);
            }
            Sprite nextSprite = LoadSprite(nextIndex); // Baru muat gambar baru

            if (isLayer1Active) // Bergantian gambar layer
            {
                yield return StartCoroutine(Crossfade(imageLayer1, imageLayer2, nextSprite));
            }
            else
            {
                yield return StartCoroutine(Crossfade(imageLayer2, imageLayer1, nextSprite));
            }

            Resources.UnloadUnusedAssets(); // Hapus aset yang tidak digunakan, hemat resource
            currentIndex = nextIndex; // Simpan index sekarang untuk dicek selanjutnya, ulangi
            isLayer1Active = !isLayer1Active; // Agar tahu sekarang menggunakan layer yang mana
        }
    }

    IEnumerator Crossfade(Image fromLayer, Image toLayer, Sprite nextSprite)
    {
        toLayer.sprite = nextSprite;
        float elapsed = 0;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime; // Selama 1.5 detik dihitung, lakukan crossfade
            float alpha = elapsed / fadeDuration; // Jumlah alpha bertambah gradual untuk memuluskan fade

            fromLayer.color = new Color(1, 1, 1, 1 - alpha); // Alpha layer sebelumnya dibuat transparan
            toLayer.color = new Color(1, 1, 1, alpha); // dan selanjutnya dimunculkan
            yield return null;
        }

        fromLayer.color = new Color(1, 1, 1, 0); // Pastikan layer sebelunya transparan dan yang sekarang muncul
        toLayer.color = new Color(1, 1, 1, 1); 
    }

    Sprite LoadSprite(int index)
    {
        // Loading gambar dari Backgrounds/bg_0, bg_1, dst. Sudah diformat namanya.
        return Resources.Load<Sprite>("Backgrounds/bg_" + index);
    }
}