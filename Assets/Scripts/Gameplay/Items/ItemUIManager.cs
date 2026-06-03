using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager instance { get; private set; } // Akan digunakan skrip lain

    [Header("UI Box Components")]
    public RectTransform itemUIBoxPanel; // Panel kotak UI item berisi icon, item, dan durasi
    public Image itemIconDisplay;
    public TMP_Text itemNameDisplay;
    public TMP_Text itemDurationDisplay;

    [Header("Duration Bar")]
    public Slider durationSlider; // Durasi akan ditampilkan dengan slider

    [Header("Animation Configuration")]
    public float targetShowY = -10f;     // Koordinat posisi turun
    public float hidePositionY = 200f;   // Koordinat posisi sembunyi
    public float slideDuration = 0.5f;   // Durasi geser panel

    private Coroutine activeUIAnimationCoroutine;
    private Coroutine activeCountdownCoroutine;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Sembunyikan panel ke atas
        if (itemUIBoxPanel != null)
        {
            itemUIBoxPanel.anchoredPosition = new Vector2(itemUIBoxPanel.anchoredPosition.x, hidePositionY);
        }
        // Sembunyikan bar durasi juga
        if (durationSlider != null) durationSlider.gameObject.SetActive(false);
    }

    public void ShowItemBox(string itemName, Sprite itemIcon, float duration)
    {
        // Masukkan data informasi item ke UI
        if (itemNameDisplay != null) itemNameDisplay.text = itemName;

        if (itemIconDisplay != null)
        {
            if (itemIcon != null)
            {
                itemIconDisplay.gameObject.SetActive(true);
                itemIconDisplay.sprite = itemIcon;
            }
            else
            {
                itemIconDisplay.gameObject.SetActive(false); // Matikan gambar jika ikon kosong
            }
        }

        // Hentikan coroutine lama jika sedang berjalan (Pengaman anti-tumpang tindih jika ambil item beruntun)
        if (activeUIAnimationCoroutine != null) StopCoroutine(activeUIAnimationCoroutine);
        if (activeCountdownCoroutine != null) StopCoroutine(activeCountdownCoroutine);

        // Jalankan animasi panel turun ke bawah dan hitung mundur durasi
        activeUIAnimationCoroutine = StartCoroutine(SlidePanelRoutine(targetShowY));
        activeCountdownCoroutine = StartCoroutine(ItemDurationCountdownRoutine(duration));
    }

    IEnumerator SlidePanelRoutine(float targetY) // Geser kotak UI ke posisi Y yg diinginkan
    {
        float elapsed = 0f;
        Vector2 startPos = itemUIBoxPanel.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, targetY);

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime; 
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            itemUIBoxPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        itemUIBoxPanel.anchoredPosition = targetPos;
    }

    IEnumerator ItemDurationCountdownRoutine(float totalDuration) // Pergeseran slider sisa waktu durasi item
    {
        float durationRemaining = totalDuration;

        // Jika item berefek instan, tampilkan 3 detik agar dapat dibaca
        if (Mathf.Approximately(totalDuration, 3f))
        {
            // Kosongkan teks durasi dan matikan slider karena instan
            if (itemDurationDisplay != null) itemDurationDisplay.text = "";
            if (durationSlider != null) durationSlider.gameObject.SetActive(false);

            yield return new WaitForSecondsRealtime(3f);
        }
        else
        {
            // Jika item berdurasi, nyalakan komponen Slider durasi
            if (durationSlider != null) 
            {
                durationSlider.gameObject.SetActive(true); durationSlider.maxValue = totalDuration;
            }
            while (durationRemaining > 0f)
            {
                if (durationSlider != null)
                { 
                    durationSlider.value = durationRemaining; // Value slider mengikuti durasi tersisa
                }
                yield return null; 
                durationRemaining -= Time.unscaledDeltaTime;
            }
        }

        // Begitu selesai, tarik ke luar layar
        activeUIAnimationCoroutine = StartCoroutine(SlidePanelRoutine(hidePositionY));
    }

    public void ForceHideUIBox()
    {
        if (activeUIAnimationCoroutine != null) StopCoroutine(activeUIAnimationCoroutine);
        if (activeCountdownCoroutine != null) StopCoroutine(activeCountdownCoroutine);
        
        if (itemUIBoxPanel != null)
        {
            // Paksa koordinat panel langsung melompat ke atas luar layar
            itemUIBoxPanel.anchoredPosition = new Vector2(itemUIBoxPanel.anchoredPosition.x, hidePositionY);
        }
    }
}